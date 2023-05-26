using BeatLeader.Models.Replay;
using System;
using System.Collections.Generic;
using System.Text;

namespace LocalLeaderboard.Services
{
    public class ReplayService
    {
        public static bool TryDecode(byte[] buffer, out BeatLeader.Models.Replay.Replay replay)
        {
            replay = null;
            try
            {
                replay = Decode(buffer);
                return replay != null;
            }
            catch
            {
                return false;
            }
        }
        public static BeatLeader.Models.Replay.Replay Decode(byte[] buffer)
        {
            int arrayLength = (int)buffer.Length;

            int pointer = 0;

            int magic = DecodeInt(buffer, ref pointer);
            byte version = buffer[pointer++];

            if (magic == 0x442d3d69 && version == 1)
            {
                BeatLeader.Models.Replay.Replay replay = new BeatLeader.Models.Replay.Replay();

                for (int a = 0; a < ((int)StructType.pauses) + 1 && pointer < arrayLength; a++)
                {
                    StructType type = (StructType)buffer[pointer++];

                    switch (type)
                    {
                        case StructType.info:
                            replay.info = DecodeInfo(buffer, ref pointer);
                            break;
                        case StructType.frames:
                            replay.frames = DecodeFrames(buffer, ref pointer);
                            break;
                        case StructType.notes:
                            replay.notes = DecodeNotes(buffer, ref pointer);
                            break;
                        case StructType.walls:
                            replay.walls = DecodeWalls(buffer, ref pointer);
                            break;
                        case StructType.heights:
                            replay.heights = DecodeHeight(buffer, ref pointer);
                            break;
                        case StructType.pauses:
                            replay.pauses = DecodePauses(buffer, ref pointer);
                            break;
                    }
                }

                return replay;
            }
            else
            {
                return null;
            }
        }

        private static ReplayInfo DecodeInfo(byte[] buffer, ref int pointer)
        {
            ReplayInfo result = new ReplayInfo();

            result.version = DecodeString(buffer, ref pointer);
            result.gameVersion = DecodeString(buffer, ref pointer);
            result.timestamp = DecodeString(buffer, ref pointer);

            result.playerID = DecodeString(buffer, ref pointer);
            result.playerName = DecodeName(buffer, ref pointer);
            result.platform = DecodeString(buffer, ref pointer);

            result.trackingSytem = DecodeString(buffer, ref pointer);
            result.hmd = DecodeString(buffer, ref pointer);
            result.controller = DecodeString(buffer, ref pointer);

            result.hash = DecodeString(buffer, ref pointer);
            result.songName = DecodeString(buffer, ref pointer);
            result.mapper = DecodeString(buffer, ref pointer);
            result.difficulty = DecodeString(buffer, ref pointer);

            result.score = DecodeInt(buffer, ref pointer);
            result.mode = DecodeString(buffer, ref pointer);
            result.environment = DecodeString(buffer, ref pointer);
            result.modifiers = DecodeString(buffer, ref pointer);
            result.jumpDistance = DecodeFloat(buffer, ref pointer);
            result.leftHanded = DecodeBool(buffer, ref pointer);
            result.height = DecodeFloat(buffer, ref pointer);

            result.startTime = DecodeFloat(buffer, ref pointer);
            result.failTime = DecodeFloat(buffer, ref pointer);
            result.speed = DecodeFloat(buffer, ref pointer);

            return result;
        }

        private static List<Frame> DecodeFrames(byte[] buffer, ref int pointer)
        {
            int length = DecodeInt(buffer, ref pointer);
            List<Frame> result = new List<Frame>();
            for (int i = 0; i < length; i++)
            {
                var frame = DecodeFrame(buffer, ref pointer);
                if (frame.time != 0 && (result.Count == 0 || frame.time != result[result.Count - 1].time))
                {
                    result.Add(frame);
                }
            }
            return result;
        }

        private static Frame DecodeFrame(byte[] buffer, ref int pointer)
        {
            Frame result = new Frame();
            result.time = DecodeFloat(buffer, ref pointer);
            result.fps = DecodeInt(buffer, ref pointer);
            result.head = DecodeEuler(buffer, ref pointer);
            result.leftHand = DecodeEuler(buffer, ref pointer);
            result.rightHand = DecodeEuler(buffer, ref pointer);

            return result;
        }

        private static List<NoteEvent> DecodeNotes(byte[] buffer, ref int pointer)
        {
            int length = DecodeInt(buffer, ref pointer);
            List<NoteEvent> result = new List<NoteEvent>();
            for (int i = 0; i < length; i++)
            {
                result.Add(DecodeNote(buffer, ref pointer));
            }
            return result;
        }

        private static List<WallEvent> DecodeWalls(byte[] buffer, ref int pointer)
        {
            int length = DecodeInt(buffer, ref pointer);
            List<WallEvent> result = new List<WallEvent>();
            for (int i = 0; i < length; i++)
            {
                WallEvent wall = new WallEvent();
                wall.wallID = DecodeInt(buffer, ref pointer);
                wall.energy = DecodeFloat(buffer, ref pointer);
                wall.time = DecodeFloat(buffer, ref pointer);
                wall.spawnTime = DecodeFloat(buffer, ref pointer);
                result.Add(wall);
            }
            return result;
        }

        private static List<AutomaticHeight> DecodeHeight(byte[] buffer, ref int pointer)
        {
            int length = DecodeInt(buffer, ref pointer);
            List<AutomaticHeight> result = new List<AutomaticHeight>();
            for (int i = 0; i < length; i++)
            {
                AutomaticHeight height = new AutomaticHeight();
                height.height = DecodeFloat(buffer, ref pointer);
                height.time = DecodeFloat(buffer, ref pointer);
                result.Add(height);
            }
            return result;
        }

        private static List<Pause> DecodePauses(byte[] buffer, ref int pointer)
        {
            int length = DecodeInt(buffer, ref pointer);
            List<Pause> result = new List<Pause>();
            for (int i = 0; i < length; i++)
            {
                Pause pause = new Pause();
                pause.duration = DecodeLong(buffer, ref pointer);
                pause.time = DecodeFloat(buffer, ref pointer);
                result.Add(pause);
            }
            return result;
        }

        private static NoteEvent DecodeNote(byte[] buffer, ref int pointer)
        {
            NoteEvent result = new NoteEvent();
            result.noteID = DecodeInt(buffer, ref pointer);
            result.eventTime = DecodeFloat(buffer, ref pointer);
            result.spawnTime = DecodeFloat(buffer, ref pointer);
            result.eventType = (NoteEventType)DecodeInt(buffer, ref pointer);
            if (result.eventType == NoteEventType.good || result.eventType == NoteEventType.bad)
            {
                result.noteCutInfo = DecodeCutInfo(buffer, ref pointer);
            }

            return result;
        }

        private static BeatLeader.Models.Replay.NoteCutInfo DecodeCutInfo(byte[] buffer, ref int pointer)
        {
            BeatLeader.Models.Replay.NoteCutInfo result = new BeatLeader.Models.Replay.NoteCutInfo();
            result.speedOK = DecodeBool(buffer, ref pointer);
            result.directionOK = DecodeBool(buffer, ref pointer);
            result.saberTypeOK = DecodeBool(buffer, ref pointer);
            result.wasCutTooSoon = DecodeBool(buffer, ref pointer);
            result.saberSpeed = DecodeFloat(buffer, ref pointer);
            result.saberDir = DecodeVector3(buffer, ref pointer);
            result.saberType = DecodeInt(buffer, ref pointer);
            result.timeDeviation = DecodeFloat(buffer, ref pointer);
            result.cutDirDeviation = DecodeFloat(buffer, ref pointer);
            result.cutPoint = DecodeVector3(buffer, ref pointer);
            result.cutNormal = DecodeVector3(buffer, ref pointer);
            result.cutDistanceToCenter = DecodeFloat(buffer, ref pointer);
            result.cutAngle = DecodeFloat(buffer, ref pointer);
            result.beforeCutRating = DecodeFloat(buffer, ref pointer);
            result.afterCutRating = DecodeFloat(buffer, ref pointer);
            return result;
        }

        private static Transform DecodeEuler(byte[] buffer, ref int pointer)
        {
            Transform result = new Transform();
            result.position = DecodeVector3(buffer, ref pointer);
            result.rotation = DecodeQuaternion(buffer, ref pointer);

            return result;
        }

        private static Vector3 DecodeVector3(byte[] buffer, ref int pointer)
        {
            Vector3 result = new Vector3();
            result.x = DecodeFloat(buffer, ref pointer);
            result.y = DecodeFloat(buffer, ref pointer);
            result.z = DecodeFloat(buffer, ref pointer);

            return result;
        }

        private static Quaternion DecodeQuaternion(byte[] buffer, ref int pointer)
        {
            Quaternion result = new Quaternion();
            result.x = DecodeFloat(buffer, ref pointer);
            result.y = DecodeFloat(buffer, ref pointer);
            result.z = DecodeFloat(buffer, ref pointer);
            result.w = DecodeFloat(buffer, ref pointer);

            return result;
        }

        private static long DecodeLong(byte[] buffer, ref int pointer)
        {
            long result = BitConverter.ToInt64(buffer, pointer);
            pointer += 8;
            return result;
        }

        private static int DecodeInt(byte[] buffer, ref int pointer)
        {
            int result = BitConverter.ToInt32(buffer, pointer);
            pointer += 4;
            return result;
        }

        private static string DecodeName(byte[] buffer, ref int pointer)
        {
            int length = BitConverter.ToInt32(buffer, pointer);
            int lengthOffset = 0;
            if (length > 0)
            {
                while (BitConverter.ToInt32(buffer, length + pointer + 4 + lengthOffset) != 6
                    && BitConverter.ToInt32(buffer, length + pointer + 4 + lengthOffset) != 5
                    && BitConverter.ToInt32(buffer, length + pointer + 4 + lengthOffset) != 8)
                {
                    lengthOffset++;
                }
            }
            string @string = Encoding.UTF8.GetString(buffer, pointer + 4, length + lengthOffset);
            pointer += length + 4 + lengthOffset;
            return @string;
        }

        private static string DecodeString(byte[] buffer, ref int pointer)
        {
            int length = BitConverter.ToInt32(buffer, pointer);
            if (length > 300 || length < 0)
            {
                pointer += 1;
                return DecodeString(buffer, ref pointer);
            }
            string @string = Encoding.UTF8.GetString(buffer, pointer + 4, length);
            pointer += length + 4;
            return @string;
        }

        private static float DecodeFloat(byte[] buffer, ref int pointer)
        {
            float result = BitConverter.ToSingle(buffer, pointer);
            pointer += 4;
            return result;
        }

        private static bool DecodeBool(byte[] buffer, ref int pointer)
        {
            bool result = BitConverter.ToBoolean(buffer, pointer);
            pointer++;
            return result;
        }

        
    }
}
