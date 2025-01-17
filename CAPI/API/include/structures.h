#pragma once
#ifndef STRUCTURES_H
#define STRUCTURES_H

#include <cstdint>
#include <array>
#include <map>

namespace THUAI5
{
    /// <summary>
    /// 道具
    /// </summary>
    enum class PropType :unsigned char
    {
        NullPropType = 0,
        addHP = 1,
        addAP = 2,
        addSpeed = 3,
        addLIFE = 4,
        minusCD = 5,
        Gem = 6,    
        Shield = 7,
        Spear = 8,
        minusSpeed = 9,
        minusAP = 10,
        addCD = 11
    };

    /// <summary>
    /// 游戏实体的形状标志
    /// </summary>
    enum class ShapeType :unsigned char
    {
        NullShapeType = 0,
        Circle = 1,
        Square = 2
    };

    /// <summary>
    /// 位置标志
    /// </summary>
    enum class PlaceType :unsigned char
    {
        Land = 0,
        Wall = 1,
        Grass1 = 2,
        Grass2 = 3,
        Grass3 = 4,
        BirthPlace = 5,
        GemWell = 6
    };

    /// <summary>
    /// 子弹
    /// </summary>
    enum class BulletType :unsigned char
    {
        NullBulletType = 0,
        CommonBullet1 = 1,
        CommonBullet2 = 2,
        FastBullet = 3,
        OrdinaryBullet = 4,
        AtomBomb = 5
    };

    /// <summary>
    /// buff
    /// </summary>
    enum class BuffType :unsigned char
    {
        NullBuffType = 0,
        MoveSpeed = 1,
        AP = 2,
        CD = 3,
        AddLIFE = 4,
        ShieldBuff = 5,
        SpearBuff = 6
    };

    /// <summary>
    /// 被动技能
    /// </summary>
    enum class PassiveSkillType :unsigned char 
    { 
        NullPassiveSkillType = 0,
        RecoverAfterBattle = 1,
        SpeedUpWhenLeavingGrass = 2,
        Vampire = 3,
        PSkill3 = 4,
        PSkill4 = 5,
        PSkill5 = 6
    };

    /// <summary>
    /// 主动技能
    /// </summary>
    enum class ActiveSkillType :unsigned char
    {
        NullActiveSkillType = 0,
        BecomeVampire = 1,
        BecomeAssassin = 2,
        NuclearWeapon = 3,
        SuperFast = 4,
        ASkill4 = 5,
        ASkill5 = 6
    };

    /// <summary>
    /// 人物
    /// </summary>
    struct Character
    {
        bool canMove;                                   // 是否可以移动
        bool isResetting;                               // 是否在复活中

        uint32_t x;                                     // x坐标
        uint32_t y;                                     // y坐标
        uint32_t bulletNum;                             // 子弹数量 
        uint32_t speed;                                 // 人物移动速度
        uint32_t life;                                  // 生命数
        uint32_t gemNum;                                // 宝石数
        uint32_t radius;                                // 圆形物体的半径或正方形物体的内切圆半径
        uint32_t CD;                                    // 回复一颗子弹需要的时间
        uint32_t lifeNum;		                        // 第几条命
        uint32_t score;                                 // 分数

        uint64_t teamID;                                // 队伍ID
        uint64_t playerID;                              // 玩家ID
        uint64_t guid;                                  // 操作方法：Client和Server互相约定guid。非负整数中，1-8这8个guid预留给8个人物，其余在子弹或道具被创造/破坏时分发和回收。Client端用向量[guid]储存物体信息和对应的控件实例。
                                                        // 0号guid存储单播模式中每人Client对应的GUID。

        double attackRange;                             // 攻击范围
        double timeUntilCommonSkillAvailable;           // 普通主动技能的冷却时间 
        double timeUntilUltimateSkillAvailable;         // 特殊主动技能的冷却时间
        double vampire;                                 // 吸血率

        std::vector<BuffType> buff;                                  // 所拥有的buff
        PropType prop;                                  // 所持有的道具
        PlaceType place;                                // 人物所在位置
        BulletType bulletType;                          // 子弹类型
        PassiveSkillType passiveSkillType;              // 持有的被动技能 
        ActiveSkillType activeSkillType;                // 持有的主动技能
    };

    /// <summary>
    /// 墙
    /// </summary>
    struct Wall
    {
        ShapeType shapeType;                            // 墙的形状（正方形）
        uint16_t radius;                                // 圆形物体的半径或正方形内切圆半径
        uint32_t x;                                     // x坐标
        uint32_t y;                                     // y坐标
        int64_t guid;                                   // guid
    };
    // 注：墙的处理机制目前还不大确定，所以先不写了（这块还需要探讨一下）

    /// <summary>
    /// 道具
    /// </summary>
    struct Prop
    {
        uint32_t x;                                     // x坐标
        uint32_t y;                                     // y坐标
        uint32_t size;                                  // 宝石大小
        uint64_t guid;                                  // guid

        double facingDirection;                         // 朝向

        PropType type;                                  // 种类
        PlaceType place;                                // 道具放置位置
    };

    /// <summary>
    /// 子弹
    /// </summary>
    struct Bullet
    {
        uint32_t x;                                     // x坐标
        uint32_t y;                                     // y坐标

        uint64_t guid;                                  // guid
        uint64_t parentTeamID;                          // 所属队伍??

        double facingDirection;                         // 朝向

        BulletType type;                                // 子弹种类
        PlaceType place;                                // 放置位置
    };

    // debug方便使用。名称可以改动

    inline std::map<THUAI5::PropType, std::string> prop_dict
    {
        { PropType::NullPropType,"NullPropType"},
        { PropType::addHP,"addHP"},
        { PropType::addAP,"addAP"},
        { PropType::addSpeed,"addSpeed"},
        { PropType::addLIFE ,"addLIFE "},
        { PropType::minusCD ,"minusCD "},
        { PropType::Gem ,"Gem "},
        { PropType::Shield ,"Shield "},
        { PropType::Spear ,"Spear "},
        { PropType::minusSpeed ,"minusSpeed "},
        { PropType::minusAP ,"minusAP "},
        { PropType::addCD ,"addCD "}
    };

    inline std::map<THUAI5::PlaceType, std::string> place_dict
    {
        { PlaceType::Land ,"Land "},
        { PlaceType::Wall ,"Wall "},
        { PlaceType::Grass1 ,"Grass1 "},
        { PlaceType::Grass2 ,"Grass2 "},
        { PlaceType::Grass3 ,"Grass3 "},
        { PlaceType::BirthPlace ,"BirthPlace "},
        { PlaceType::GemWell ,"GemWell "}
    };

    inline std::map<THUAI5::BuffType, std::string> buff_dict
    {
        { BuffType::NullBuffType ,"NullBuffType "},
        { BuffType::MoveSpeed ,"MoveSpeed "},
        { BuffType::AP ,"AP "},
        { BuffType::CD ,"CD "},
        { BuffType::AddLIFE ,"AddLIFE "},
        { BuffType::ShieldBuff ,"ShieldBuff "},
        { BuffType::SpearBuff ,"SpearBuff "},
    };

    inline std::map<THUAI5::BulletType, std::string> bullet_dict
    {
        { BulletType::NullBulletType ,"NullBulletType "},
        { BulletType::CommonBullet1 ,"CommonBullet1 "},
        { BulletType::CommonBullet2 ,"CommonBullet2 "},
        { BulletType::FastBullet ,"FastBullet "},
        { BulletType::OrdinaryBullet ,"OrdinaryBullet "},
        { BulletType::AtomBomb ,"AtomBomb "}
    };

    inline std::map<THUAI5::ActiveSkillType, std::string> active_dict
    {
        { ActiveSkillType::NullActiveSkillType ,"NullActiveSkillType "},
        { ActiveSkillType::BecomeVampire ,"BecomeVampire "},
        { ActiveSkillType::BecomeAssassin ,"BecomeAssassin "},
        { ActiveSkillType::NuclearWeapon ,"NuclearWeapon "},
        { ActiveSkillType::SuperFast ,"SuperFast "},
        { ActiveSkillType::ASkill4 ,"ASkill4 "},
        { ActiveSkillType::ASkill5 ,"ASkill5 "}
    };

    inline std::map<THUAI5::PassiveSkillType, std::string> passive_dict
    {
        { PassiveSkillType::NullPassiveSkillType ,"NullPassiveSkillType "},
        { PassiveSkillType::RecoverAfterBattle ,"RecoverAfterBattle "},
        { PassiveSkillType::SpeedUpWhenLeavingGrass ,"SpeedUpWhenLeavingGrass "},
        { PassiveSkillType::Vampire ,"Vampire "},
        { PassiveSkillType::PSkill3 ,"PSkill3 "},
        { PassiveSkillType::PSkill4 ,"PSkill4 "},
        { PassiveSkillType::PSkill5 ,"PSkill5 "}
    };
}

#endif // !STRUCTURES_H
