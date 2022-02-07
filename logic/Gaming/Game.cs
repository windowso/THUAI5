﻿using System;
using System.Threading;
using System.Collections.Generic;
using GameClass.GameObj;
using Preparation.Utility;
using Preparation.GameData;
using Timothy.FrameRateTask;
using Preparation.Interface;

namespace Gaming
{
    public partial class Game
    {
        public struct PlayerInitInfo
        {
            public uint birthPointIndex;
            public long teamID;
            public long playerID;
            public PassiveSkillType passiveSkill;
            public ActiveSkillType commonSkill;
            public PlayerInitInfo(uint birthPointIndex, long teamID, long playerID, PassiveSkillType passiveSkill, ActiveSkillType commonSkill)
            {
                this.birthPointIndex = birthPointIndex;
                this.teamID = teamID;
                this.passiveSkill = passiveSkill;
                this.commonSkill = commonSkill;
                this.playerID = playerID;
            }
        }

        private readonly List<Team> teamList;
        public List<Team> TeamList => teamList;
        private readonly Map gameMap;
        public Map GameMap => gameMap;
        private readonly int numOfTeam;
        public long AddPlayer(PlayerInitInfo playerInitInfo)
        {
            if (!Team.teamExists(playerInitInfo.teamID))
                /*  || !MapInfo.ValidBirthPointIdx(playerInitInfo.birthPointIdx)
                  || gameMap.BirthPointList[playerInitInfo.birthPointIdx].Parent != null)*/
                return GameObj.invalidID;

            XYPosition pos = gameMap.BirthPointList[playerInitInfo.birthPointIndex].Position;
            //Console.WriteLine($"x,y: {pos.x},{pos.y}");
            Character newPlayer = new(pos, GameData.characterRadius, gameMap.GetPlaceType(pos), playerInitInfo.passiveSkill, playerInitInfo.commonSkill);
            gameMap.BirthPointList[playerInitInfo.birthPointIndex].Parent = newPlayer;
            gameMap.GameObjLockDict["player"].EnterWriteLock();
            try
            {
                gameMap.GameObjDict["player"].Add(newPlayer);
            }
            finally
            {
                gameMap.GameObjLockDict["player"].ExitWriteLock();
            }
            //Console.WriteLine($"GameObjDict["player"] length:{gameMap.GameObjDict["player"].Count}");
            teamList[(int)playerInitInfo.teamID].AddPlayer(newPlayer);
            newPlayer.TeamID = playerInitInfo.teamID;
            newPlayer.PlayerID = playerInitInfo.playerID;

            new Thread  //检查人物位置，同时人物装弹。
            (
                () =>
                {
                    while (!gameMap.Timer.IsGaming)
                        Thread.Sleep(newPlayer.CD);
                    long lastTime = Environment.TickCount64;
                    new FrameRateTaskExecutor<int>
                    (
                        loopCondition: () => gameMap.Timer.IsGaming,
                        loopToDo: () =>
                        {
                            if (!newPlayer.IsResetting)
                            {
                                //if (newPlayer.Place != PlaceType.Invisible)
                                newPlayer.Place = gameMap.GetPlaceType(newPlayer.Position);

                                long nowTime = Environment.TickCount64;
                                if (nowTime - lastTime >= newPlayer.CD)
                                {
                                    _ = newPlayer.TryAddBulletNum();
                                    lastTime = nowTime;
                                }
                            }
                        },
                        timeInterval: GameData.checkInterval,
                        finallyReturn: () => 0
                    )
                    {
                        AllowTimeExceed = true
                        /*MaxTolerantTimeExceedCount = 5,
                        TimeExceedAction = exceedTooMuch =>
                        {
                            if (exceedTooMuch) Console.WriteLine("The computer runs too slow that it cannot check the color below the player in time!");
                        }*/
                    }.Start();
                }
            )
            { IsBackground = true }.Start();

            return newPlayer.ID;
        }
        public bool StartGame(int milliSeconds)
        {
            if (gameMap.Timer.IsGaming)
                return false;
            gameMap.GameObjLockDict["player"].EnterReadLock();
            try
            {
                foreach (Character player in gameMap.GameObjDict["player"])
                {
                    player.CanMove = true;

                    player.AddShield(GameData.shieldTimeAtBirth);
                }
            }
            finally { gameMap.GameObjLockDict["player"].ExitReadLock(); }


            propManager.StartProducing();
            gemManager.StartProducingGem();
            new Thread
            (
                () =>
                {
                    new FrameRateTaskExecutor<int>
                    (
                        loopCondition: () => gameMap.Timer.IsGaming,
                        loopToDo: () =>
                        {
                            gameMap.GameObjLockDict["bullet"].EnterWriteLock();  //检查子弹位置
                            try
                            {
                                foreach(var bullet in gameMap.GameObjDict["bullet"])
                                {
                                    bullet.Place = gameMap.GetPlaceType(bullet.Position);
                                }
                            }
                            finally { gameMap.GameObjLockDict["bullet"].ExitWriteLock(); }
                        },
                        timeInterval: GameData.checkInterval,
                        finallyReturn: () => 0
                    )
                    {
                        AllowTimeExceed = true
                    }.Start();
                }
            )
            { IsBackground = true }.Start();
            //开始游戏
            if (!gameMap.Timer.StartGame(milliSeconds))
                return false;

            EndGame(); //游戏结束时要做的事

            //清除所有对象
            gameMap.GameObjLockDict["player"].EnterWriteLock();
            try
            {
                foreach (Character player in gameMap.GameObjDict["player"])
                {
                    player.CanMove = false;
                }
                gameMap.GameObjDict["player"].Clear();
            }
            finally { gameMap.GameObjLockDict["player"].ExitWriteLock(); }
            gameMap.GameObjLockDict["bullet"].EnterWriteLock();
            try
            {
                gameMap.GameObjDict["bullet"].Clear();
            }
            finally { gameMap.GameObjLockDict["bullet"].ExitWriteLock(); }
            gameMap.GameObjLockDict["prop"].EnterWriteLock();
            try
            {
                gameMap.GameObjDict["prop"].Clear();
            }
            finally { gameMap.GameObjLockDict["prop"].ExitWriteLock(); }
            gameMap.GameObjLockDict["gem"].EnterWriteLock();
            try
            {
                gameMap.GameObjDict["gem"].Clear();
            }
            finally { gameMap.GameObjLockDict["gem"].ExitWriteLock(); }

            return true;
        }

        public void EndGame()
        {
            gameMap.GameObjLockDict["player"].EnterWriteLock();
            try
            {
                foreach (var player in gameMap.GameObjDict["player"])  //这里始终运行不下去，为什么？？？
                {
                    gemManager.UseAllGem((Character)player);
                    Console.WriteLine("Fuck");
                }
            }
            finally { gameMap.GameObjLockDict["player"].ExitWriteLock(); }

        }
        public void MovePlayer(long playerID, int moveTimeInMilliseconds, double angle)
        {
            if (!gameMap.Timer.IsGaming)
                return;
            Character? player = gameMap.FindPlayer(playerID);
            if (player != null)
            {
                moveManager.MovePlayer(player, moveTimeInMilliseconds, angle);
#if DEBUG
                Console.WriteLine($"PlayerID:{playerID} move to ({player.Position.x},{player.Position.y})!");
#endif
            }
            else
            {
#if DEBUG
                Console.WriteLine($"PlayerID:{playerID} player does not exists!");
#endif
            }
        }
        public void Attack(long playerID, double angle)
        {
            if (!gameMap.Timer.IsGaming)
                return;
            Character? player = gameMap.FindPlayer(playerID);
            if (player != null)
            {
                _ = attackManager.Attack(player, angle);
            }
        }
        public void UseGem(long playerID, int num)
        {
            if (!gameMap.Timer.IsGaming)
                return;
            Character? player = gameMap.FindPlayer(playerID);
            if(player!=null)
            {
                gemManager.UseGem(player, num);
                return;
            }
        }
        public void ThrowGem(long playerID, int moveMilliTime,double angle ,int size = 1)
        {
            if (!gameMap.Timer.IsGaming)
                return;
            Character? player = gameMap.FindPlayer(playerID);
            if (player != null)
            {
                gemManager.ThrowGem(player,moveMilliTime,angle,size);
                return;
            }
        }
        public bool PickGem(long playerID)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? player = gameMap.FindPlayer(playerID);
            if(player!=null)
            {
                return gemManager.PickGem(player);
            }
            return false;
        }
        public void UseProp(long playerID)
        {
            if (!gameMap.Timer.IsGaming)
                return;
            Character? player = gameMap.FindPlayer(playerID);
            if(player!=null)
            {
                propManager.UseProp(player);
            }
        }
        public void ThrowProp(long playerID,int timeInmillionSeconds,double angle)
        {
            if (!gameMap.Timer.IsGaming)
                return;
            Character? player = gameMap.FindPlayer(playerID);
            if (player != null)
            {
                propManager.ThrowProp(player, timeInmillionSeconds, angle);
            }
        }
        public bool PickProp(long playerID,PropType propType=PropType.Null)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? player = gameMap.FindPlayer(playerID);
            if (player != null)
            {
                return propManager.PickProp(player, propType);
            }
            return false;
        }

        public bool UseCommonSkill(long playerID)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? player = gameMap.FindPlayer(playerID);
            if (player != null)
            {
                return skillManager.UseCommonSkill(player);
            }
            else return false;
        }

        public void AllPlayerUsePassiveSkill()
        {
            if (!gameMap.Timer.IsGaming)
                return;
            gameMap.GameObjLockDict["player"].EnterWriteLock();
            try
            {
                foreach (Character player in gameMap.GameObjDict["player"])
                {
                    skillManager.UsePassiveSkill(player);
                }
            }
            finally { gameMap.GameObjLockDict["player"].ExitWriteLock(); }
        }

        public int GetTeamScore(long teamID)
        {
            return teamList[(int)teamID].Score;
        }
        public List<IGameObj> GetGameObj()
        {
            var gameObjList = new List<IGameObj>();
            gameMap.GameObjLockDict["player"].EnterReadLock();
            try
            {
                gameObjList.AddRange(gameMap.GameObjDict["player"]);
            }
            finally { gameMap.GameObjLockDict["player"].ExitReadLock(); }

            gameMap.GameObjLockDict["bullet"].EnterReadLock();
            try
            {
                gameObjList.AddRange(gameMap.GameObjDict["bullet"]);
            }
            finally { gameMap.GameObjLockDict["bullet"].ExitReadLock(); }

            gameMap.GameObjLockDict["prop"].EnterReadLock();
            try
            {
                gameObjList.AddRange(gameMap.GameObjDict["prop"]);
            }
            finally { gameMap.GameObjLockDict["prop"].ExitReadLock(); }

            gameMap.GameObjLockDict["gem"].EnterReadLock();
            try
            {
                gameObjList.AddRange(gameMap.GameObjDict["gem"]);
            }
            finally { gameMap.GameObjLockDict["gem"].ExitReadLock(); }

            return gameObjList;
        }
        public Game(uint[,] mapResource, int numOfTeam)
        {
            //if (numOfTeam > maxTeamNum) throw new TeamNumOverFlowException();

            gameMap = new Map(mapResource);

            //加入队伍
            this.numOfTeam = numOfTeam;
            teamList = new List<Team>();
            for (int i = 0; i < numOfTeam; ++i)
            {
                teamList.Add(new Team());
            }

            skillManager = new SkillManager();
            attackManager = new AttackManager(gameMap);
            moveManager = new MoveManager(gameMap);
            propManager = new PropManager(gameMap);
            gemManager = new GemManager(gameMap);
        }
    }
}
