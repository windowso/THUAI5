﻿using Preparation.Interface;
using Preparation.Utility;

namespace Preparation.GameObj
{
    public abstract class GameObj:IGameObj
    {
        public enum GameObjType
        {
            Character = 0,
            Obj = 1
        }
        protected readonly object gameObjLock = new object();
        protected readonly XYPosition birthPos;

        public long ID { get;}

        private XYPosition position;
        public XYPosition Position 
        { 
            get => position; 
            protected set 
            { 
                lock (gameObjLock) 
                { 
                    position = value; 
                } 
            } 
        }
        public abstract bool IsRigid { get; }

        private double facingDirection = 0;
        public double FacingDirection 
        {
            get => facingDirection; 
            set 
            { 
                lock (gameObjLock) 
                    facingDirection = value; 
            } 
        }
        public abstract ShapeType Shape { get; }
        
        private bool canMove;
        public bool CanMove
        {
            get => canMove;
            set
            {
                lock(gameObjLock)
                {
                    canMove = value;
                }
            }
        }
        
        private bool isMoving;
        public bool IsMoving
        {
            get => isMoving;
            set
            {
                lock(gameObjLock)
                {
                    isMoving = value;
                }
            }
        }

        private bool isResetting;
        public bool IsResetting
        {
            get => isResetting;
            set
            {
                lock(gameObjLock)
                {
                    isResetting = value;
                }
            }
        }
        public bool IsAvailable => !IsMoving && CanMove && !IsResetting;    //是否能接收指令
        public int Radius { get; }

        private PlaceType place;
        public PlaceType Place
        {
            get => place;
            set
            {
                lock(gameObjLock)
                {
                    place = value;
                }
            }
        }

        public virtual bool CanSee(GameObj obj)
        {
            if (obj.Place == PlaceType.Invisible) //先判断是否隐身
                return false;
            if (obj.Place == PlaceType.Land)
                return true;
            if (obj.Place == this.Place)
                return true;
            return false;
        }
        
        public GameObj(XYPosition initPos,int initRadius,PlaceType initPlace)
        {
            this.birthPos = initPos;
            this.Radius = initRadius;
            this.place = initPlace;
        }
    }
}