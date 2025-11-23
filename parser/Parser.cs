using HandHistoryParser.exception;
using HandHistoryParser.model;
using HandHistoryParser.utils;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace HandHistoryParser.parser
{
    internal class Parser(ILogger<Parser> logger)
    {
        private const string HOLE_CARDS_SECTION_HEADER = "HOLE CARDS";
        private const string FLOP_SECTION_HEADER = "FLOP";
        private const string TERN_SECTION_HEADER = "TURN";
        private const string RIVER_SECTION_HEADER = "RIVER";
        private const string SHOW_DOWN_SECTION_HEADER = "SHOW DOWN";
        private const string SUMMARY_SECTION_HEADER = "SUMMARY";
        private const string HAND_NUMBER_TEXT = "Hand #";
        private const string SEAT_TEXT = "Seat";
        private const string DEALT_TO_TEXT = "Dealt to";
        private const string ROWS_DELIMETER = "\r\n";
        private const int PLAYER_INFO_PARTS_LENGTH = 6;
        private const int PLAYER_INFO_SEAT_NUMBER_PART_INDEX = 1;
        private const int PLAYER_INFO_NICKNAME_PART_INDEX = 2;
        private const int PLAYER_INFO_STACK_PART_INDEX = 3;
        private const int DEALT_TO_HERO_NICKNAME_PART_INDEX = 2;
        private const string HAND_HOSTORIES_DELIMETER = "\r\n\r\n\r\n\r\n";

        private readonly ILogger<Parser> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void ParseZipAndWriteOutput(string zipFilePath)
        {
            string archiveName = Path.GetFileNameWithoutExtension(zipFilePath);
            string dateString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string outputDirectory = Path.Combine("resources", "parsed");
            string outputFileName = $"{dateString}_{archiveName}.txt";
            string outputFilePath = Path.Combine(outputDirectory, outputFileName);

            Directory.CreateDirectory(outputDirectory);

            using StreamWriter writer = new(outputFilePath);
            using ZipArchive archive = ZipFile.OpenRead(zipFilePath);
            List<HandHistoryData> parsedData = ParseZipArchive(archive);
            writer.WriteLine(string.Join("\r\n", parsedData));
        }

        private List<HandHistoryData> ParseZipArchive(ZipArchive archive)
        {
            List<HandHistoryData> parsedData = [];
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        using StreamReader reader = new(entry.Open());
                        string content = reader.ReadToEnd();
                        parsedData.AddRange(ParseFileContent(content, HAND_HOSTORIES_DELIMETER));
                    }
                    catch (Exception ex)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            _logger.LogError("Error processing {entry.FullName}: {ex.Message}", entry.FullName, ex.Message);
                        }
                    }
                }
            }
            return parsedData;
        }

        private static List<HandHistoryData> ParseFileContent(string content, string delimeter)
        {
            return [.. content.Split(delimeter).Select(e => Parser.Parse(e))];
        }

        internal static HandHistoryData Parse(string handHistory)
        {
            string[] sections = handHistory.Split("***");
            return ParseSections(sections);
        }

        private static HandHistoryData ParseSections(string[] sections)
        {
            HandHistoryData data = new();
            data = ParseHeaderSection(data, sections[0]);

            for (int i = 1; i < sections.Length; i += 2)
            {
                data = ParseSection(data, sections[i], sections[i + 1]);
            }
            return data;
        }

        private static HandHistoryData ParseSection(HandHistoryData data, string sectionName, string section)
        {
            return sectionName.Trim() switch
            {
                HOLE_CARDS_SECTION_HEADER => ParseHoleCardsSection(data, section),
                _ => data,
            };
        }

        private static HandHistoryData ParseHoleCardsSection(HandHistoryData data, string section)
        {
            List<string> dealtToRows = [.. section.Split(ROWS_DELIMETER).Where(e => e.Trim().StartsWith(DEALT_TO_TEXT))];
            if (dealtToRows.Count != 1)
            {
                throw new ArgumentException("The number of rows with '" + DEALT_TO_TEXT + "' is " + dealtToRows.Count, nameof(section));
            }

            Player hero = ParseCardsDealtToHero(dealtToRows.First());
            return data.WithNewPlayers([.. data.Players.Select(p => p.Equals(hero) ? p.updateCards(hero.Cards) : p)]);
        }

        private static Player ParseCardsDealtToHero(string dealtToRow)
        {
            string[] dealtToParts = dealtToRow.Split(" ");
            string heroNickname = dealtToParts[DEALT_TO_HERO_NICKNAME_PART_INDEX];

            int startIndex = dealtToRow.IndexOf('[') + 1;
            int endIndex = dealtToRow.IndexOf(']');

            if (startIndex == - 1 || endIndex == -1 )
            {
                throw new DataNotFoundException("Could not find hero cards");
            }

            List<Card> heroCards = [.. dealtToRow[startIndex..endIndex]
                .Split(" ")
                .Select(pair => new Card(pair[0], pair[1]))];

            return new Player(-1, heroNickname, decimal.Zero, heroCards);
        }

        private static HandHistoryData ParseHeaderSection(HandHistoryData data, string? headerSection)
        {
            if (String.IsNullOrEmpty(headerSection)) { return data; }

            long handHistoryNumber = ParseHandHistoryNumber(headerSection);
            List<Player> players = ParseHandHistoryPlayers(headerSection);
            
            return data
                .WithNewHandHistoryNumber(handHistoryNumber)
                .WithNewPlayers(players);
        }

        private static List<Player> ParseHandHistoryPlayers(string headerSection)
        {
            List<Player> players = [];
            players.AddRange([.. headerSection.Split(ROWS_DELIMETER)
                .Where(row => row.StartsWith(SEAT_TEXT))
                .Select(ParsePlayerInfo)]);
            return players;
        }

        private static Player ParsePlayerInfo(string playerInfoRow)
        {
            string[] playerInfoParts = playerInfoRow.Trim().Split(" ");
            //if (playerInfoParts.Length != PLAYER_INFO_PARTS_LENGTH)
            //{
            //    throw new ArgumentException("Trimmed value of playerInfoRow includes not " + PLAYER_INFO_PARTS_LENGTH + " parts.");
            //}

            int seatNumber = ParsePlayerSeatNumber(playerInfoParts);
            string nickname = ParsePlayerNickName(playerInfoParts);
            decimal stack = ParsePlayerStack(playerInfoParts);

            return new Player(seatNumber, nickname, stack, []);
        }

        private static decimal ParsePlayerStack(string[] playerInfoParts)
        {
            return Converter.ParseDecimal(playerInfoParts[PLAYER_INFO_STACK_PART_INDEX], 2);
        }

        private static string ParsePlayerNickName(string[] playerInfoParts)
        {
            return playerInfoParts[PLAYER_INFO_NICKNAME_PART_INDEX];
        }

        private static int ParsePlayerSeatNumber(string[] playerInfoParts)
        {
            return Converter.ParseInt(playerInfoParts[PLAYER_INFO_SEAT_NUMBER_PART_INDEX]);
        }

        private static long ParseHandHistoryNumber(string section)
        {
            int handNumberTextIndex = section.IndexOf(HAND_NUMBER_TEXT);
            if (handNumberTextIndex == -1)
            {
                throw new DataNotFoundException("'" + HAND_NUMBER_TEXT + "' Not Found in '" + section + "'.");
            }

            return Converter.ParseLong(section, handNumberTextIndex + HAND_NUMBER_TEXT.Length);
        }
    }
}
