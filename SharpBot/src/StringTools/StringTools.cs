using System.Text.RegularExpressions;

public static class StringTools
{
    /// <summary>
    /// 将中文数字转换成阿拉伯数字
    /// </summary>
    /// <param name="cnNumber"></param>
    /// <returns></returns>
    public static int ConvertToDigit(string cnNumber)
    {
        int result = 0;

        int temp = 0;

        foreach (char c in cnNumber)

        {
            int temp1 = ToDigit(c);

            if (temp1 == 10000)

            {
                result += temp;

                result *= 10000;

                temp = 0;
            }

            else if (temp1 > 9)

            {
                if (temp1 == 10 && temp == 0) temp = 1;

                result += temp * temp1;

                temp = 0;
            }

            else temp = temp1;
        }

        result += temp;

        return result;
    }


    /// <summary>
    /// 将中文数字转换成阿拉伯数字
    /// </summary>
    /// <param name="cn"></param>
    /// <returns></returns>
    public static int ToDigit(char cn)
    {
        int number = 0;

        switch (cn)

        {
            case '壹':

            case '一':

                number = 1;

                break;

            case '两':

            case '贰':

            case '二':

                number = 2;

                break;

            case '叁':

            case '三':

                number = 3;

                break;

            case '肆':

            case '四':

                number = 4;

                break;

            case '伍':

            case '五':

                number = 5;

                break;

            case '陆':

            case '六':

                number = 6;

                break;

            case '柒':

            case '七':

                number = 7;

                break;

            case '捌':

            case '八':

                number = 8;

                break;

            case '玖':

            case '九':

                number = 9;

                break;

            case '拾':

            case '十':

                number = 10;

                break;

            case '佰':

            case '百':

                number = 100;

                break;

            case '仟':

            case '千':

                number = 1000;

                break;

            case '萬':

            case '万':

                number = 10000;

                break;

            case '零':

            default:

                number = 0;

                break;
        }

        return number;
    }


    /// <summary>
    /// 将中文数字转换成阿拉伯数字
    /// </summary>
    /// <param name="cnDigit"></param>
    /// <returns></returns>
    public static long ToLong(string cnDigit)
    {
        long result = 0;

        string[] str = cnDigit.Split('亿');

        result = ConvertToDigit(str[0]);

        if (str.Length > 1)

        {
            result *= 100000000;

            result += ConvertToDigit(str[1]);
        }

        return result;
    }


    /// <summary>
    /// This class implements string comparison algorithm
    /// based on character pair similarity
    /// Source: http://www.catalysoft.com/articles/StrikeAMatch.html
    /// </summary>

    #region SimilarityTool

    /// <summary>
    /// Compares the two strings based on letter pair matches
    /// </summary>
    /// <param name="str1"></param>
    /// <param name="str2"></param>
    /// <returns>The percentage match from 0.0 to 1.0 where 1.0 is 100%</returns>
    public static double CompareStrings(string str1, string str2)
    {
        List<string> pairs1 = WordLetterPairs(str1.ToUpper());
        List<string> pairs2 = WordLetterPairs(str2.ToUpper());

        int intersection = 0;
        int union = pairs1.Count + pairs2.Count;

        for (int i = 0; i < pairs1.Count; i++)
        {
            for (int j = 0; j < pairs2.Count; j++)
            {
                if (pairs1[i] == pairs2[j])
                {
                    intersection++;
                    pairs2.RemoveAt(
                        j); //Must remove the match to prevent "GGGG" from appearing to match "GG" with 100% success

                    break;
                }
            }
        }

        return (2.0 * intersection) / union;
    }

    /// <summary>
    /// Gets all letter pairs for each
    /// individual word in the string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static List<string> WordLetterPairs(string str)
    {
        List<string> AllPairs = new List<string>();

        // Tokenize the string and put the tokens/words into an array
        string[] Words = Regex.Split(str, @"\s");

        // For each word
        for (int w = 0; w < Words.Length; w++)
        {
            if (!string.IsNullOrEmpty(Words[w]))
            {
                // Find the pairs of characters
                String[] PairsInWord = LetterPairs(Words[w]);

                for (int p = 0; p < PairsInWord.Length; p++)
                {
                    AllPairs.Add(PairsInWord[p]);
                }
            }
        }

        return AllPairs;
    }

    /// <summary>
    /// Generates an array containing every
    /// two consecutive letters in the input string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string[] LetterPairs(string str)
    {
        int numPairs = str.Length - 1;

        string[] pairs = new string[numPairs];

        for (int i = 0; i < numPairs; i++)
        {
            pairs[i] = str.Substring(i, 2);
        }

        return pairs;
    }

    #endregion
}