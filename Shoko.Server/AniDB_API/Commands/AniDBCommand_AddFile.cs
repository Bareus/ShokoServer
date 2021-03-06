﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Shoko.Models.Enums;
using Shoko.Models.Interfaces;

namespace AniDBAPI.Commands
{
    public class AniDBCommand_AddFile : AniDBUDPCommand, IAniDBUDPCommand
    {
        public IHash FileData;
        public bool ReturnIsWatched;
        public DateTime? WatchedDate;
        public AniDBFile_State? State;
        public int MyListID;

        public string GetKey()
        {
            return "AniDBCommand_AddFile" + FileData.ED2KHash;
        }

        public virtual enHelperActivityType GetStartEventType()
        {
            return enHelperActivityType.AddingFile;
        }

        public virtual enHelperActivityType Process(ref Socket soUDP,
            ref IPEndPoint remoteIpEndPoint, string sessionID, Encoding enc)
        {
            ProcessCommand(ref soUDP, ref remoteIpEndPoint, sessionID, enc);

            // handle 555 BANNED and 598 - UNKNOWN COMMAND
            switch (ResponseCode)
            {
                case 598: return enHelperActivityType.UnknownCommand_598;
                case 555: return enHelperActivityType.Banned_555;
            }
            if (errorOccurred) return enHelperActivityType.NoSuchFile;

            string sMsgType = socketResponse.Substring(0, 3);
            switch (sMsgType)
            {
                case "210": return enHelperActivityType.FileAdded;
                case "310":
                {
                    //file already exists: read 'watched' status
                    string[] arrResult = socketResponse.Split('\n');
                    if (arrResult.Length >= 2)
                    {
                        string[] arrStatus = arrResult[1].Split('|');
                        int.TryParse(arrStatus[0], out MyListID);

                        int state = int.Parse(arrStatus[6]);
                        State = (AniDBFile_State) state;

                        int viewdate = int.Parse(arrStatus[7]);
                        ReturnIsWatched = viewdate > 0;

                        if (ReturnIsWatched)
                        {
                            DateTime utcDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            utcDate = utcDate.AddSeconds(viewdate);

                            WatchedDate = utcDate.ToLocalTime();
                        }
                        else
                        {
                            WatchedDate = null;
                        }
                    }
                    return enHelperActivityType.FileAlreadyExists;
                }
                case "311": return enHelperActivityType.UpdatingFile;
                case "320": return enHelperActivityType.NoSuchFile;
                case "411": return enHelperActivityType.NoSuchFile;
                case "502": return enHelperActivityType.LoginFailed;
                case "501": return enHelperActivityType.LoginRequired;
            }

            return enHelperActivityType.FileDoesNotExist;
        }

        public AniDBCommand_AddFile()
        {
            commandType = enAniDBCommandType.AddFile;
        }

        public void Init(IHash fileData, AniDBFile_State fileState)
        {
            FileData = fileData;

            commandID = fileData.Info;

            commandText = "MYLISTADD size=" + fileData.FileSize;
            commandText += "&ed2k=" + fileData.ED2KHash;
            commandText += "&viewed=0";
            commandText += "&state=" + (int) fileState;
        }

        public void Init(int animeID, int episodeNumber, AniDBFile_State fileState)
        {
            // MYLISTADD aid={int4 aid}&generic=1&epno={int4 episode number}

            commandText = "MYLISTADD aid=" + animeID;
            commandText += "&generic=1";
            commandText += "&epno=" + episodeNumber;
            commandText += "&viewed=0";
            commandText += "&state=" + (int) fileState;
        }
    }
}
