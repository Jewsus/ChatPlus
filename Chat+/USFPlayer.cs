using System;

namespace ChatHookTutorial2
{
    public class USFPlayer
    {
        public int UserID;

        public string Prefix;

        public string Suffix;

        public byte R = 0;

        public byte G = 0;

        public byte B = 0;

        public string ChatColor
        {
            get
            {
                return string.Format("{0},{1},{2}", R.ToString("D3"), G.ToString("D3"), B.ToString("D3"));
            }
            set
            {
                bool flag = value != null;
                if (flag)
                {
                    string[] array = value.Split(new char[]
                    {
                        ','
                    });
                    bool flag2 = 3 == array.Length;
                    if (flag2)
                    {
                        byte r;
                        byte g;
                        byte b;
                        bool flag3 = byte.TryParse(array[0], out r) && byte.TryParse(array[1], out g) && byte.TryParse(array[2], out b);
                        if (flag3)
                        {
                            R = r;
                            G = g;
                            B = b;
                        }
                    }
                }
            }
        }

        public USFPlayer(int userid, string prefix, string suffix, string chatcolor)
        {
            UserID = userid;
            Prefix = prefix;
            Suffix = suffix;
            ChatColor = chatcolor;
        }
    }
}
