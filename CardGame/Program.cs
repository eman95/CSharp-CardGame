using System.Data;
using System.Text;

bool runProgram = true;

while(runProgram)
{
    CardGame cardGame = new CardGame();
    cardGame.runCardGame();

    Console.WriteLine("\nDo you want to play again?\n1 - Yes\nAny key - No");
    int userInput = 0;
    int.TryParse(Console.ReadLine(), out userInput);

    if (userInput != 1)
    {
        runProgram = false;
    }
}

//cardGame class
class CardGame
{
    Dictionary<int, string> cardsMap = new Dictionary<int, string>();
    string[] cards = new string[52];
    DataTable playerCards = new DataTable();

    string[] cardSuits = { "@", "#", "^", "*" };
    string[] cardValues = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

    //run cardgame 
    public void runCardGame()
    {
        generateCards();
        randomizeCards();
        distributeCards();
        getCardCombinations();
        getWinner();
    }

    //function to generate card deck from smallest card '2@' to largest card 'A*'
    //assign key to each card
    //key can be used to determine the highest card value later on
    void generateCards()
    {
        int i = 0;

        foreach (string cardValue in cardValues)
        {
            foreach (string cardSuit in cardSuits)
            {
                cardsMap.Add(i++, cardValue + cardSuit);
            }
        }
    }

    //add the deck of cards to an array
    //shuffle the cards
    void randomizeCards()
    {
        List<string> cardList = new List<string>();

        foreach (string card in cardsMap.Values)
        {
            cardList.Add(card);
        }

        cards = cardList.ToArray();

        Random random = new Random();
        cards = cards.OrderBy(x => random.Next()).ToArray();
    }

    //function to distribute the cards to each player
    //combine all the cards that each player got into a single column
    void distributeCards()
    {
        playerCards.Columns.Add("playerNum", typeof(string));

        for (int i = 1; i <= 13; i++)
        {
            playerCards.Columns.Add("card" + i, typeof(string));
        }

        playerCards.Columns.Add("playerCards", typeof(string));

        for (int i = 1; i <= 4; i++)
        {
            DataRow newrow = playerCards.NewRow();
            newrow["playerNum"] = "Player " + i;

            playerCards.Rows.Add(newrow);
        }

        int cardPosition = 0;

        foreach (DataColumn col in playerCards.Columns)
        {
            if (col.ColumnName == "playerNum") continue;

            foreach (DataRow row in playerCards.Rows)
            {
                if (col.ColumnName.StartsWith("card"))
                {
                    row[col.ColumnName] = cards[cardPosition];
                    row["playerCards"] += (row["playerCards"].ToString() == "" ? cards[cardPosition] : "," + cards[cardPosition]);
                }

                cardPosition++;
            }
        }
    }

    //function to get the highest number of cards with the same alphanumeric part
    void getCardCombinations()
    {
        playerCards.Columns.Add("highestCount", typeof(int));
        playerCards.Columns.Add("cardCombination", typeof(string));


        foreach (DataRow row in playerCards.Rows)
        {
            string playerCardString = (string)row["playerCards"];
            string[] playerCardsArray = playerCardString.Split(",");
            string cardCombinationRow = "";
            int cardCountRow = 0;

            foreach (string cardValue in cardValues)
            {
                List<string> cardCombination = (from string playerCard in playerCardsArray
                                                where playerCard.StartsWith(cardValue)
                                                select playerCard).ToList();

                int cardCount = cardCombination.Count();

                if (cardCount >= cardCountRow)
                {
                    cardCountRow = cardCount;
                    cardCombinationRow = String.Join(",", cardCombination);
                }
            }

            row["highestCount"] = cardCountRow;
            row["cardCombination"] = cardCombinationRow;

        }
    }

    //function to get the index of the winning player
    //if only 1 player has the highest number of cards with the same alphanumeric part, that row index will then be used to determine the winner
    //if more than 1 player has the highest number of cards with the same alphanumeric part, it will go to another function that will determine the highest card value held by that player
    void getWinner()
    {
        int maxHighestCount = playerCards.AsEnumerable().Max(x => (int)x["highestCount"]);
        int countWinner = playerCards.AsEnumerable().Where(x => ((int)x["highestCount"]) == maxHighestCount).ToList().Count;
        int rowIndex = -1;

        if (countWinner > 1)
        {
            rowIndex = getHighestCardValue(maxHighestCount);
        }
        else
        {
            rowIndex = playerCards.AsEnumerable().Where(x => ((int)x["highestCount"]) == maxHighestCount).Select(x => playerCards.Rows.IndexOf(x)).FirstOrDefault();
        }

        if (rowIndex != -1) printPlayers(rowIndex);

    }

    //function to determine the player that has the highest card value
    //return the row index for that player
    int getHighestCardValue(int maxHighestCount)
    {
        playerCards.Columns.Add("highestCardValue", typeof(int));

        foreach (DataRow row in playerCards.Rows)
        {
            int highestCardValueRow = -1;

            if ((int)row["highestCount"] != maxHighestCount)
            {
                row["highestCardValue"] = 0;
                continue;
            }

            string cardCombination = (string)row["cardCombination"];
            string[] cardCombinationSplit = cardCombination.Split(",");

            foreach (string card in cardCombinationSplit)
            {
                int highestCardValue = cardsMap.FirstOrDefault(x => x.Value == card).Key;

                if (highestCardValue >= highestCardValueRow)
                {
                    highestCardValueRow = highestCardValue;
                }
            }

            row["highestCardValue"] = highestCardValueRow;
        }

        int maxValue = playerCards.AsEnumerable().Max(x => (int)x["highestCardValue"]);
        int rowIndex = playerCards.AsEnumerable().Where(x => ((int)x["highestCardValue"]) == maxValue).Select(x => playerCards.Rows.IndexOf(x)).FirstOrDefault();

        return rowIndex;
    }

    //function to print all the cards held by the players, the highest number of cards with the same alphanumeric part, and the winner of that match
    void printPlayers(int rowIndex)
    {
        StringBuilder strBuilder = new StringBuilder();

        foreach (DataRow row in playerCards.Rows)
        {
            strBuilder.Append("\n" + row["playerNum"] + ": ");
            strBuilder.Append(row["playerCards"] + String.Format(" || ({0})", row["cardCombination"]));

            if (playerCards.Rows.IndexOf(row) == rowIndex)
            {
                strBuilder.Append(" Winner!");
            }
        }

        Console.WriteLine(strBuilder.ToString());
    }
}


