using ClansV2;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;

namespace ChatHookTutorial2
{
    [ApiVersion(1, 23)]
    public class ChatHookTutorial2 : TerrariaPlugin
    {
        private int chatIndex;

        private bool[] rainbow;

        private Color[] colors;

        public IDbConnection db;

        public Dictionary<int, USFPlayer> Players = new Dictionary<int, USFPlayer>();

        public override Version Version
        {
            get
            {
                return new Version("1.0.0.0");
            }
        }

        public override string Name
        {
            get
            {
                return "RainbowChat & Clan Name & Color";
            }
        }

        public override string Author
        {
            get
            {
                return "Olink & DarkunderdoG";
            }
        }

        public override string Description
        {
            get
            {
                return "RainbowChat & Clan Chat";
            }
        }

        public ChatHookTutorial2(Main game) : base(game)
        {
            Order = 2;
            chatIndex = 0;
            colors = new Color[]
            {
                Color.Red,
                Color.Orange,
                Color.Yellow,
                Color.Green,
                Color.Aqua,
                Color.Blue,
                Color.Purple,
                Color.Violet,
                Color.Pink
            };
            rainbow = new bool[255];
            int num;
            for (int i = 0; i < 255; i = num + 1)
            {
                rainbow[i] = false;
                num = i;
            }
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(this, new HookHandler<ServerChatEventArgs>(OnChat));
            ServerApi.Hooks.GameInitialize.Register(this, new HookHandler<EventArgs>(OnInitialize));
            ServerApi.Hooks.ServerLeave.Register(this, new HookHandler<LeaveEventArgs>(OnLeave));
        }

        public void OnInitialize(EventArgs args)
        {
            SetupDb();
            loadDatabase();
            Commands.ChatCommands.Add(new Command("usf.set", new CommandDelegate(USFCommand), new string[]
            {
                "us"
            }));
            Commands.ChatCommands.Add(new Command("rainbow", new CommandDelegate(RainbowToggle), new string[]
            {
                "rainbow",
                "rb"
            }));
        }

        private void OnLeave(LeaveEventArgs args)
        {
            rainbow[args.Who] = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerChat.Deregister(this, new HookHandler<ServerChatEventArgs>(OnChat));
                ServerApi.Hooks.ServerLeave.Deregister(this, new HookHandler<LeaveEventArgs>(OnLeave));
                ServerApi.Hooks.GameInitialize.Deregister(this, new HookHandler<EventArgs>(OnInitialize));
            }
            base.Dispose(disposing);
        }

        private void RainbowToggle(CommandArgs args)
        {
            bool flag = args.Parameters.Count == 1;
            if (flag)
            {
                bool flag2 = args.Parameters[0].ToLower() == "true";
                if (flag2)
                {
                    rainbow[args.Player.Index] = true;
                }
                else
                {
                    bool flag3 = args.Parameters[0].ToLower() == "false";
                    if (flag3)
                    {
                        rainbow[args.Player.Index] = false;
                    }
                }
                args.Player.SendMessage(string.Format("You are {0} talking in rainbow.", rainbow[args.Player.Index] ? "now" : "not"), Color.Green);
            }
            else
            {
                args.Player.SendMessage("Usage: /rainbow [true/false]", Color.Red);
            }
        }

        private void OnChat(ServerChatEventArgs args)
        {
            bool handled = args.Handled;
            if (!handled)
            {
                TSPlayer tSPlayer = TShock.Players[args.Who];
                bool flag = args.Text.StartsWith(TShock.Config.CommandSpecifier) || args.Text.StartsWith(TShock.Config.CommandSilentSpecifier) || tSPlayer.mute || !tSPlayer.IsLoggedIn;
                if (!flag)
                {
                    ClanMember clanMember = Clans.CM.FindClanMember(tSPlayer);
                    Clan clan = Clans.CM.FindClanByName(clanMember.ClanName);
                    bool flag2 = rainbow[args.Who] && clan.ClanName == "";
                    if (flag2)
                    {
                        Color[] arg_CD_0 = colors;
                        int num = chatIndex;
                        chatIndex = num + 1;
                        Chat(arg_CD_0[num], args.Text, tSPlayer);
                        bool flag3 = chatIndex > colors.Length - 1;
                        if (flag3)
                        {
                            chatIndex = 0;
                        }
                        args.Handled = true;
                    }
                    else
                    {
                        bool flag4 = clan.ClanName == "";
                        if (flag4)
                        {
                            bool flag5 = Players.ContainsKey(tSPlayer.User.ID);
                            if (flag5)
                            {
                                TShock.Utils.Broadcast(string.Format(TShock.Config.ChatFormat, new object[]
                                {
                                    tSPlayer.Group.Name,
                                    (Players[tSPlayer.User.ID].Prefix != null) ? Players[tSPlayer.User.ID].Prefix : tSPlayer.Group.Prefix,
                                    tSPlayer.Name,
                                    (Players[tSPlayer.User.ID].Suffix != null) ? Players[tSPlayer.User.ID].Suffix : tSPlayer.Group.Suffix,
                                    args.Text
                                }), (Players[tSPlayer.User.ID].ChatColor != string.Format("000,000,000", new object[0])) ? new Color(Players[tSPlayer.User.ID].R, Players[tSPlayer.User.ID].G, Players[tSPlayer.User.ID].B) : new Color(tSPlayer.Group.R, tSPlayer.Group.G, tSPlayer.Group.B));
                                args.Handled = true;
                            }
                        }
                        else
                        {
                            bool flag6 = !rainbow[args.Who];
                            if (flag6)
                            {
                                Color color = Clans.CM.FromStringToColor(clan.ChatColor);
                                Chat2(clanMember.ClanName, color, args.Text, tSPlayer);
                                bool flag7 = chatIndex > colors.Length - 1;
                                if (flag7)
                                {
                                    chatIndex = 0;
                                }
                                args.Handled = true;
                            }
                            else
                            {
                                string arg_375_1 = clanMember.ClanName;
                                Color[] arg_369_0 = colors;
                                int num = chatIndex;
                                chatIndex = num + 1;
                                Chat2(arg_375_1, arg_369_0[num], args.Text, tSPlayer);
                                bool flag8 = chatIndex > colors.Length - 1;
                                if (flag8)
                                {
                                    chatIndex = 0;
                                }
                                args.Handled = true;
                            }
                        }
                    }
                }
            }
        }

        private void Chat(Color color, string message, TSPlayer tsplr)
        {
            bool flag = Players.ContainsKey(tsplr.User.ID);
            if (flag)
            {
                TShock.Utils.Broadcast(string.Format(TShock.Config.ChatFormat, new object[]
                {
                    tsplr.Group.Name,
                    (Players[tsplr.User.ID].Prefix != null) ? Players[tsplr.User.ID].Prefix : tsplr.Group.Prefix,
                    tsplr.Name,
                    (Players[tsplr.User.ID].Suffix != null) ? Players[tsplr.User.ID].Suffix : tsplr.Group.Suffix,
                    message
                }), color);
            }
            else
            {
                TShock.Utils.Broadcast(string.Format(TShock.Config.ChatFormat, new object[]
                {
                    tsplr.Group.Name,
                    tsplr.Group.Prefix,
                    tsplr.Name,
                    tsplr.Group.Suffix,
                    message
                }), color);
            }
        }

        private void Chat2(string clan, Color color, string message, TSPlayer tsplr)
        {
            bool flag = Players.ContainsKey(tsplr.User.ID);
            if (flag)
            {
                TShock.Utils.Broadcast(string.Format(TShock.Config.ChatFormat, new object[]
                {
                    tsplr.Group.Name,
                    (Players[tsplr.User.ID].Prefix != null) ? Players[tsplr.User.ID].Prefix : tsplr.Group.Prefix,
                    tsplr.Name,
                    clan,
                    message
                }), color);
            }
            else
            {
                TShock.Utils.Broadcast(string.Format(TShock.Config.ChatFormat, new object[]
                {
                    tsplr.Group.Name,
                    tsplr.Group.Prefix,
                    tsplr.Name,
                    clan,
                    message
                }), color);
            }
        }

        public void USFCommand(CommandArgs args)
        {
            bool flag = args.Parameters.Count < 1;
            if (flag)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax:");
                args.Player.SendErrorMessage("{0}us prefix <prefix>", new object[]
                {
                    TShock.Config.CommandSpecifier
                });
                args.Player.SendErrorMessage("{0}us suffix <suffix>", new object[]
                {
                    TShock.Config.CommandSpecifier
                });
                args.Player.SendErrorMessage("{0}us color <rrr,ggg,bbb>", new object[]
                {
                    TShock.Config.CommandSpecifier
                });
                args.Player.SendErrorMessage("{0}us remove <prefix/suffix/color>", new object[]
                {
                    TShock.Config.CommandSpecifier
                });
            }
            else
            {
                User userByName = TShock.Users.GetUserByName(args.Player.Name);
                bool flag2 = userByName == null;
                if (flag2)
                {
                    args.Player.SendErrorMessage("No users under that name.");
                }
                else
                {
                    string a = args.Parameters[0].ToLower();
                    if (!(a == "prefix"))
                    {
                        if (!(a == "suffix"))
                        {
                            if (!(a == "color"))
                            {
                                if (!(a == "remove"))
                                {
                                    if (!(a == "help"))
                                    {
                                        args.Player.SendErrorMessage("Invalid subcommand. Type {0}us help for a list of valid commands.", new object[]
                                        {
                                            TShock.Config.CommandSpecifier
                                        });
                                    }
                                    else
                                    {
                                        args.Player.SendInfoMessage("{0}us prefix <prefix>", new object[]
                                        {
                                            TShock.Config.CommandSpecifier
                                        });
                                        args.Player.SendInfoMessage("{0}us suffix <suffix>", new object[]
                                        {
                                            TShock.Config.CommandSpecifier
                                        });
                                        args.Player.SendInfoMessage("{0}us color <rrr,ggg,bbb>", new object[]
                                        {
                                            TShock.Config.CommandSpecifier
                                        });
                                        args.Player.SendInfoMessage("{0}us remove <prefix/suffix/color>", new object[]
                                        {
                                            TShock.Config.CommandSpecifier
                                        });
                                    }
                                }
                                else
                                {
                                    bool flag3 = args.Parameters.Count != 2;
                                    if (flag3)
                                    {
                                        args.Player.SendErrorMessage("Invalid syntax: {0}us remove <prefix/suffix/color>", new object[]
                                        {
                                            TShock.Config.CommandSpecifier
                                        });
                                    }
                                    else
                                    {
                                        string a2 = args.Parameters[1].ToLower();
                                        if (!(a2 == "prefix"))
                                        {
                                            if (!(a2 == "suffix"))
                                            {
                                                if (a2 == "color")
                                                {
                                                    bool flag4 = !Players.ContainsKey(userByName.ID) || Players[userByName.ID].ChatColor == string.Format("000,000,000", new object[0]);
                                                    if (flag4)
                                                    {
                                                        args.Player.SendErrorMessage("This user doesn't have a color to remove.");
                                                    }
                                                    else
                                                    {
                                                        removeUserColor(userByName.ID);
                                                        args.Player.SendSuccessMessage("Removed {0}'s color.", new object[]
                                                        {
                                                            userByName.Name
                                                        });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bool flag5 = !Players.ContainsKey(userByName.ID) || Players[userByName.ID].Suffix == null;
                                                if (flag5)
                                                {
                                                    args.Player.SendErrorMessage("This user doesn't have a suffix to remove.");
                                                }
                                                else
                                                {
                                                    removeUserSuffix(userByName.ID);
                                                    args.Player.SendSuccessMessage("Removed {0}'s suffix.", new object[]
                                                    {
                                                        userByName.Name
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bool flag6 = !Players.ContainsKey(userByName.ID) || Players[userByName.ID].Prefix == null;
                                            if (flag6)
                                            {
                                                args.Player.SendErrorMessage("This user doesn't have a prefix to remove.");
                                            }
                                            else
                                            {
                                                removeUserPrefix(userByName.ID);
                                                args.Player.SendSuccessMessage("Removed {0}'s prefix.", new object[]
                                                {
                                                    userByName.Name
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                bool flag7 = args.Parameters.Count == 2;
                                if (flag7)
                                {
                                    string text = args.Parameters[1];
                                    string[] array = text.Split(new char[]
                                    {
                                        ','
                                    });
                                    byte b;
                                    byte b2;
                                    byte b3;
                                    bool flag8 = array.Length == 3 && byte.TryParse(array[0], out b) && byte.TryParse(array[1], out b2) && byte.TryParse(array[2], out b3);
                                    if (flag8)
                                    {
                                        try
                                        {
                                            setUserColor(userByName.ID, text);
                                            args.Player.SendSuccessMessage("Set \"{0}\"'s color to: \"{1}\"", new object[]
                                            {
                                                userByName.Name,
                                                text
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            args.Player.SendErrorMessage(ex.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    bool flag9 = args.Parameters.Count == 1;
                                    if (flag9)
                                    {
                                        bool flag10 = !Players.ContainsKey(userByName.ID) || Players[userByName.ID].ChatColor == string.Format("000,000,000", new object[0]);
                                        if (flag10)
                                        {
                                            args.Player.SendErrorMessage("\"{0}\" has no chat color to display.", new object[]
                                            {
                                                userByName.Name
                                            });
                                        }
                                        else
                                        {
                                            args.Player.SendSuccessMessage("\"{0}\"'s chat color is: \"{1}\"", new object[]
                                            {
                                                userByName.Name,
                                                Players[userByName.ID].ChatColor
                                            });
                                        }
                                    }
                                    else
                                    {
                                        args.Player.SendErrorMessage("Invalid color: {0}us color [rrr,ggg,bbb]", new object[]
                                        {
                                            TShock.Config.CommandSpecifier
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            bool flag11 = args.Parameters.Count == 2;
                            if (flag11)
                            {
                                string text2 = string.Join(" ", new string[]
                                {
                                    args.Parameters[1]
                                });
                                setUserSuffix(userByName.ID, text2);
                                args.Player.SendSuccessMessage("Set \"{0}\"'s suffix to: \"{1}\"", new object[]
                                {
                                    userByName.Name,
                                    text2
                                });
                            }
                            else
                            {
                                bool flag12 = args.Parameters.Count == 1;
                                if (flag12)
                                {
                                    bool flag13 = !Players.ContainsKey(userByName.ID) || Players[userByName.ID].Suffix == null;
                                    if (flag13)
                                    {
                                        args.Player.SendErrorMessage("\"{0}\" has no suffix to display.", new object[]
                                        {
                                            userByName.Name
                                        });
                                    }
                                    else
                                    {
                                        args.Player.SendSuccessMessage("\"{0}\"'s suffix is: \"{1}\"", new object[]
                                        {
                                            userByName.Name,
                                            Players[userByName.ID].Suffix
                                        });
                                    }
                                }
                                else
                                {
                                    args.Player.SendErrorMessage("Invalid syntax: {0}us suffix [suffix]", new object[]
                                    {
                                        TShock.Config.CommandSpecifier
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        bool flag14 = args.Parameters.Count == 2;
                        if (flag14)
                        {
                            string text3 = string.Join(" ", new string[]
                            {
                                args.Parameters[1]
                            });
                            setUserPrefix(userByName.ID, text3);
                            args.Player.SendSuccessMessage("Set \"{0}\"'s prefix to: \"{1}\"", new object[]
                            {
                                userByName.Name,
                                text3
                            });
                        }
                        else
                        {
                            bool flag15 = args.Parameters.Count == 1;
                            if (flag15)
                            {
                                bool flag16 = !Players.ContainsKey(userByName.ID) || Players[userByName.ID].Prefix == null;
                                if (flag16)
                                {
                                    args.Player.SendErrorMessage("\"{0}\" has no prefix to display.", new object[]
                                    {
                                        userByName.Name
                                    });
                                }
                                else
                                {
                                    args.Player.SendSuccessMessage("\"{0}\"'s prefix is: \"{1}\"", new object[]
                                    {
                                        userByName.Name,
                                        Players[userByName.ID].Prefix
                                    });
                                }
                            }
                            else
                            {
                                args.Player.SendErrorMessage("Invalid syntax: {0}us prefix [prefix]", new object[]
                                {
                                    TShock.Config.CommandSpecifier
                                });
                            }
                        }
                    }
                }
            }
        }

        public void SetupDb()
        {
            string lower = TShock.Config.StorageType.ToLower();
            if (!(lower == "mysql"))
            {
                if (lower == "sqlite")
                    db = new SqliteConnection(string.Format("uri=file://{0},Version=3", Path.Combine(TShock.SavePath, "tshock.sqlite")));
            }
            else
            {
                string[] strArray = TShock.Config.MySqlHost.Split(':');
                MySqlConnection mySqlConnection = new MySqlConnection();
                mySqlConnection.ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};", strArray[0], strArray.Length == 1 ? "3306" : strArray[1], TShock.Config.MySqlDbName, TShock.Config.MySqlUsername, TShock.Config.MySqlPassword);
                db = mySqlConnection;
            }
            new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator()).EnsureTableStructure(new SqlTable("UserSpecificFunctions", new SqlColumn[4]
            {
        new SqlColumn("UserID", MySqlDbType.Int32)
        {
          Primary = true,
          Unique = true,
          Length = new int?(6)
        },
        new SqlColumn("Prefix", MySqlDbType.Text)
        {
          Length = new int?(25)
        },
        new SqlColumn("Suffix", MySqlDbType.Text)
        {
          Length = new int?(25)
        },
        new SqlColumn("ChatColor", MySqlDbType.Text)
            }));
        }

        public void loadDatabase()
        {
            Players.Clear();
            using (QueryResult queryResult = DbExt.QueryReader(db, "SELECT * FROM UserSpecificFunctions", new object[0]))
            {
                while (queryResult.Read())
                {
                    int num = queryResult.Get<int>("UserID");
                    string prefix = queryResult.Get<string>("Prefix");
                    string suffix = queryResult.Get<string>("Suffix");
                    string chatcolor = queryResult.Get<string>("ChatColor");
                    Players.Add(num, new USFPlayer(num, prefix, suffix, chatcolor));
                }
            }
        }

        public void setUserPrefix(int userid, string prefix)
        {
            bool flag = !Players.ContainsKey(userid);
            if (flag)
            {
                Players.Add(userid, new USFPlayer(userid, prefix, null, string.Format("000,000,000", new object[0])));
                DbExt.Query(db, "INSERT INTO UserSpecificFunctions (UserID, Prefix, Suffix, ChatColor) VALUES (@0, @1, @2, @3);", new object[]
                {
                    userid.ToString(),
                    prefix,
                    null,
                    string.Format("000,000,000", new object[0])
                });
            }
            else
            {
                Players[userid].Prefix = prefix;
                DbExt.Query(db, "UPDATE UserSpecificFunctions SET Prefix=@0 WHERE UserID=@1;", new object[]
                {
                    prefix,
                    userid.ToString()
                });
            }
        }

        public void setUserSuffix(int userid, string suffix)
        {
            bool flag = !Players.ContainsKey(userid);
            if (flag)
            {
                Players.Add(userid, new USFPlayer(userid, null, suffix, string.Format("000,000,000", new object[0])));
                DbExt.Query(db, "INSERT INTO UserSpecificFunctions (UserID, Prefix, Suffix, ChatColor) VALUES (@0, @1, @2, @3);", new object[]
                {
                    userid.ToString(),
                    null,
                    suffix,
                    string.Format("000,000,000", new object[0])
                });
            }
            else
            {
                Players[userid].Suffix = suffix;
                DbExt.Query(db, "UPDATE UserSpecificFunctions SET Suffix=@0 WHERE UserID=@1;", new object[]
                {
                    suffix,
                    userid.ToString()
                });
            }
        }

        public void setUserColor(int userid, string chatcolor)
        {
            bool flag = !Players.ContainsKey(userid);
            if (flag)
            {
                Players.Add(userid, new USFPlayer(userid, null, null, chatcolor));
                DbExt.Query(db, "INSERT INTO UserSpecificFunctions (UserID, Prefix, Suffix, ChatColor) VALUES (@0, @1, @2, @3);", new object[]
                {
                    userid.ToString(),
                    null,
                    null,
                    chatcolor
                });
            }
            else
            {
                Players[userid].ChatColor = chatcolor;
                DbExt.Query(db, "UPDATE UserSpecificFunctions SET ChatColor=@0 WHERE UserID=@1;", new object[]
                {
                    chatcolor,
                    userid.ToString()
                });
            }
        }

        public void removeUserPrefix(int userid)
        {
            Players[userid].Prefix = null;
            DbExt.Query(db, "UPDATE UserSpecificFunctions SET Prefix=null WHERE UserID=@0;", new object[]
            {
                userid.ToString()
            });
        }

        public void removeUserSuffix(int userid)
        {
            Players[userid].Suffix = null;
            DbExt.Query(db, "UPDATE UserSpecificFunctions SET Suffix=null WHERE UserID=@0;", new object[]
            {
                userid.ToString()
            });
        }

        public void removeUserColor(int userid)
        {
            Players[userid].ChatColor = string.Format("000,000,000", new object[0]);
            DbExt.Query(db, "UPDATE UserSpecificFunctions SET ChatColor=@0 WHERE UserID=@1;", new object[]
            {
                string.Format("000,000,000", new object[0]),
                userid.ToString()
            });
        }
    }
}
