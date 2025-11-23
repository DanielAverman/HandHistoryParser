using HandHistoryParser.parser;

const String testHandHistory = 
    "PokerStars Hand #93405882771:  Hold'em No Limit ($0.10/$0.25 USD) - 2013/02/03 1:16:19 EET [2013/02/02 18:16:19 ET]\r\n" +
    "Table 'Stobbe III' 6-max Seat #4 is the button\r\n" +
    "Seat 1: VakaLuks ($26.87 in chips) \r\n" +
    "Seat 2: BigBlindBets ($29.73 in chips) \r\n" +
    "Seat 3: Jamol121 ($17.66 in chips) \r\n" +
    "Seat 4: ubbikk ($26.06 in chips) \r\n" +
    "Seat 5: RicsiTheKid ($25 in chips) \r\n" +
    "Seat 6: angrypaca ($26.89 in chips) \r\n" +
    "RicsiTheKid: posts small blind $0.10\r\n" +
    "angrypaca: posts big blind $0.25\r\n" +
    "*** HOLE CARDS ***\r\n" +
    "Dealt to angrypaca [6d As]\r\n" +
    "VakaLuks: folds \r\n" +
    "BigBlindBets: folds \r\n" +
    "Jamol121: calls $0.25\r\n" +
    "ubbikk: folds \r\n" +
    "RicsiTheKid: folds \r\n" +
    "angrypaca: checks \r\n" +
    "*** FLOP *** [5s Qs 3c]\r\n" +
    "angrypaca: checks \r\n" +
    "Jamol121: checks \r\n" +
    "*** TURN *** [5s Qs 3c] [8d]\r\n" +
    "angrypaca: checks \r\n" +
    "Jamol121: bets $0.25\r\n" + 
    "angrypaca: folds \r\n" +
    "Uncalled bet ($0.25) returned to Jamol121\r\n" +
    "Jamol121 collected $0.57 from pot\r\n" +
    "*** SUMMARY ***\r\n" +
    "Total pot $0.60 | Rake $0.03 \r\n" +
    "Board [5s Qs 3c 8d]\r\n" +
    "Seat 1: VakaLuks folded before Flop (didn't bet)\r\n" +
    "Seat 2: BigBlindBets folded before Flop (didn't bet)\r\n" +
    "Seat 3: Jamol121 collected ($0.57)\r\n" +
    "Seat 4: ubbikk (button) folded before Flop (didn't bet)\r\n" +
    "Seat 5: RicsiTheKid (small blind) folded before Flop\r\n" +
    "Seat 6: angrypaca (big blind) folded on the Turn";

Console.Write(Parser.Parse(testHandHistory));


    