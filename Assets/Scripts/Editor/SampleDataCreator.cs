#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.Editor
{
    /// <summary>
    /// Creates sample ScriptableObject data for testing.
    /// Access via menu: IdleViking → Create Sample Data
    /// </summary>
    public class SampleDataCreator : EditorWindow
    {
        private const string DataPath = "Assets/Data";

        [MenuItem("IdleViking/Create Sample Data")]
        public static void CreateAllSampleData()
        {
            EnsureDirectory(DataPath);
            EnsureDirectory($"{DataPath}/Resources");
            EnsureDirectory($"{DataPath}/Buildings");
            EnsureDirectory($"{DataPath}/Vikings");
            EnsureDirectory($"{DataPath}/Equipment");
            EnsureDirectory($"{DataPath}/Enemies");
            EnsureDirectory($"{DataPath}/Dungeons");
            EnsureDirectory($"{DataPath}/Farm");
            EnsureDirectory($"{DataPath}/Progression");

            // Create in order of dependencies
            var producers = CreateResourceData();
            CreateBuildingData(producers);
            CreateVikingData();
            CreateEquipmentData();
            var enemies = CreateEnemyData();
            CreateDungeonData(enemies);
            CreateFarmData();
            CreateMilestoneData();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Sample Data Created",
                "All sample data has been created in Assets/Data/\n\n" +
                "Now assign the databases to GameManager:\n" +
                "- ResourceDatabase\n" +
                "- BuildingDatabase\n" +
                "- VikingDatabase\n" +
                "- EquipmentDatabase\n" +
                "- EnemyDatabase\n" +
                "- DungeonDatabase\n" +
                "- FarmDatabase\n" +
                "- MilestoneDatabase", "OK");
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static ResourceProducerData[] CreateResourceData()
        {
            // Gold Mine
            var goldMine = CreateAsset<ResourceProducerData>($"{DataPath}/Resources/Producer_GoldMine.asset");
            goldMine.producerId = "gold_mine";
            goldMine.displayName = "Gold Mine";
            goldMine.resourceType = ResourceType.Gold;
            goldMine.baseProductionRate = 1.0;
            goldMine.upgradeMultiplier = 0.5;
            goldMine.baseCost = 50;

            // Lumber Mill
            var lumberMill = CreateAsset<ResourceProducerData>($"{DataPath}/Resources/Producer_LumberMill.asset");
            lumberMill.producerId = "lumber_mill";
            lumberMill.displayName = "Lumber Mill";
            lumberMill.resourceType = ResourceType.Wood;
            lumberMill.baseProductionRate = 2.0;
            lumberMill.upgradeMultiplier = 0.5;
            lumberMill.baseCost = 40;

            // Quarry
            var quarry = CreateAsset<ResourceProducerData>($"{DataPath}/Resources/Producer_Quarry.asset");
            quarry.producerId = "quarry";
            quarry.displayName = "Stone Quarry";
            quarry.resourceType = ResourceType.Stone;
            quarry.baseProductionRate = 1.5;
            quarry.upgradeMultiplier = 0.5;
            quarry.baseCost = 60;

            // Farm
            var farm = CreateAsset<ResourceProducerData>($"{DataPath}/Resources/Producer_Farm.asset");
            farm.producerId = "food_farm";
            farm.displayName = "Food Farm";
            farm.resourceType = ResourceType.Food;
            farm.baseProductionRate = 3.0;
            farm.upgradeMultiplier = 0.5;
            farm.baseCost = 30;

            // Database
            var db = CreateAsset<ResourceDatabase>($"{DataPath}/ResourceDatabase.asset");
            db.Producers.Add(goldMine);
            db.Producers.Add(lumberMill);
            db.Producers.Add(quarry);
            db.Producers.Add(farm);

            Debug.Log("[SampleData] Created Resource data");
            return new[] { goldMine, lumberMill, quarry, farm };
        }

        private static void CreateBuildingData(ResourceProducerData[] producers)
        {
            // Town Hall
            var townHall = CreateAsset<BuildingData>($"{DataPath}/Buildings/Building_TownHall.asset");
            townHall.buildingId = "town_hall";
            townHall.displayName = "Town Hall";
            townHall.description = "The heart of your village. Unlocks new features.";
            townHall.maxLevel = 10;
            townHall.buildCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 100 },
                new ResourceCost { resourceType = ResourceType.Wood, amount = 50 }
            };
            townHall.costExponent = 1.5;

            // Gold Mine Building
            var goldMineBuilding = CreateAsset<BuildingData>($"{DataPath}/Buildings/Building_GoldMine.asset");
            goldMineBuilding.buildingId = "gold_mine";
            goldMineBuilding.displayName = "Gold Mine";
            goldMineBuilding.description = "Produces gold over time.";
            goldMineBuilding.maxLevel = 20;
            goldMineBuilding.linkedProducer = producers[0]; // goldMine producer
            goldMineBuilding.buildCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 50 },
                new ResourceCost { resourceType = ResourceType.Wood, amount = 25 }
            };
            goldMineBuilding.costExponent = 1.3;

            // Lumber Mill Building
            var lumberMillBuilding = CreateAsset<BuildingData>($"{DataPath}/Buildings/Building_LumberMill.asset");
            lumberMillBuilding.buildingId = "lumber_mill";
            lumberMillBuilding.displayName = "Lumber Mill";
            lumberMillBuilding.description = "Produces wood over time.";
            lumberMillBuilding.maxLevel = 20;
            lumberMillBuilding.linkedProducer = producers[1]; // lumberMill producer
            lumberMillBuilding.buildCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 40 }
            };
            lumberMillBuilding.costExponent = 1.3;

            // Barracks
            var barracks = CreateAsset<BuildingData>($"{DataPath}/Buildings/Building_Barracks.asset");
            barracks.buildingId = "barracks";
            barracks.displayName = "Barracks";
            barracks.description = "Train and house your vikings.";
            barracks.maxLevel = 15;
            barracks.buildCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 200 },
                new ResourceCost { resourceType = ResourceType.Wood, amount = 150 },
                new ResourceCost { resourceType = ResourceType.Stone, amount = 100 }
            };
            barracks.costExponent = 1.4;
            barracks.prerequisiteBuilding = townHall;
            barracks.prerequisiteLevel = 2;

            // Database
            var db = CreateAsset<BuildingDatabase>($"{DataPath}/BuildingDatabase.asset");
            db.Buildings.Add(townHall);
            db.Buildings.Add(goldMineBuilding);
            db.Buildings.Add(lumberMillBuilding);
            db.Buildings.Add(barracks);

            Debug.Log("[SampleData] Created Building data");
        }

        private static void CreateVikingData()
        {
            // Warrior
            var warrior = CreateAsset<VikingData>($"{DataPath}/Vikings/Viking_Warrior.asset");
            warrior.vikingId = "warrior";
            warrior.displayName = "Viking Warrior";
            warrior.description = "A brave warrior ready for battle.";
            warrior.rarity = Rarity.Common;
            warrior.baseHP = 100;
            warrior.baseATK = 15;
            warrior.baseDEF = 10;
            warrior.baseSPD = 10;
            warrior.growthHP = 10;
            warrior.growthATK = 2;
            warrior.growthDEF = 1;
            warrior.growthSPD = 1;
            warrior.recruitCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 100 },
                new ResourceCost { resourceType = ResourceType.Food, amount = 50 }
            };

            // Archer
            var archer = CreateAsset<VikingData>($"{DataPath}/Vikings/Viking_Archer.asset");
            archer.vikingId = "archer";
            archer.displayName = "Viking Archer";
            archer.description = "Skilled with the bow, fast but fragile.";
            archer.rarity = Rarity.Common;
            archer.baseHP = 70;
            archer.baseATK = 20;
            archer.baseDEF = 5;
            archer.baseSPD = 15;
            archer.growthHP = 7;
            archer.growthATK = 3;
            archer.growthDEF = 1;
            archer.growthSPD = 2;
            archer.recruitCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 120 },
                new ResourceCost { resourceType = ResourceType.Food, amount = 40 }
            };

            // Berserker
            var berserker = CreateAsset<VikingData>($"{DataPath}/Vikings/Viking_Berserker.asset");
            berserker.vikingId = "berserker";
            berserker.displayName = "Berserker";
            berserker.description = "Unstoppable rage, high damage but low defense.";
            berserker.rarity = Rarity.Rare;
            berserker.baseHP = 120;
            berserker.baseATK = 25;
            berserker.baseDEF = 5;
            berserker.baseSPD = 12;
            berserker.growthHP = 12;
            berserker.growthATK = 4;
            berserker.growthDEF = 1;
            berserker.growthSPD = 1;
            berserker.recruitCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 500 },
                new ResourceCost { resourceType = ResourceType.Food, amount = 200 }
            };

            // Shieldmaiden
            var shieldmaiden = CreateAsset<VikingData>($"{DataPath}/Vikings/Viking_Shieldmaiden.asset");
            shieldmaiden.vikingId = "shieldmaiden";
            shieldmaiden.displayName = "Shieldmaiden";
            shieldmaiden.description = "Legendary defender of the clan.";
            shieldmaiden.rarity = Rarity.Epic;
            shieldmaiden.baseHP = 150;
            shieldmaiden.baseATK = 12;
            shieldmaiden.baseDEF = 25;
            shieldmaiden.baseSPD = 8;
            shieldmaiden.growthHP = 15;
            shieldmaiden.growthATK = 2;
            shieldmaiden.growthDEF = 4;
            shieldmaiden.growthSPD = 1;
            shieldmaiden.recruitCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 1000 },
                new ResourceCost { resourceType = ResourceType.Food, amount = 400 }
            };

            // Database
            var db = CreateAsset<VikingDatabase>($"{DataPath}/VikingDatabase.asset");
            db.Vikings.Add(warrior);
            db.Vikings.Add(archer);
            db.Vikings.Add(berserker);
            db.Vikings.Add(shieldmaiden);

            Debug.Log("[SampleData] Created Viking data");
        }

        private static void CreateEquipmentData()
        {
            // Iron Sword
            var ironSword = CreateAsset<EquipmentData>($"{DataPath}/Equipment/Equip_IronSword.asset");
            ironSword.equipmentId = "iron_sword";
            ironSword.displayName = "Iron Sword";
            ironSword.description = "A basic but reliable sword.";
            ironSword.slot = EquipmentSlot.Weapon;
            ironSword.rarity = Rarity.Common;
            ironSword.bonusATK = 5;
            ironSword.craftCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 50 },
                new ResourceCost { resourceType = ResourceType.Iron, amount = 20 }
            };

            // Steel Axe
            var steelAxe = CreateAsset<EquipmentData>($"{DataPath}/Equipment/Equip_SteelAxe.asset");
            steelAxe.equipmentId = "steel_axe";
            steelAxe.displayName = "Steel Axe";
            steelAxe.description = "A powerful two-handed axe.";
            steelAxe.slot = EquipmentSlot.Weapon;
            steelAxe.rarity = Rarity.Uncommon;
            steelAxe.bonusATK = 12;
            steelAxe.craftCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 150 },
                new ResourceCost { resourceType = ResourceType.Iron, amount = 50 }
            };

            // Leather Armor
            var leatherArmor = CreateAsset<EquipmentData>($"{DataPath}/Equipment/Equip_LeatherArmor.asset");
            leatherArmor.equipmentId = "leather_armor";
            leatherArmor.displayName = "Leather Armor";
            leatherArmor.description = "Light armor for quick movement.";
            leatherArmor.slot = EquipmentSlot.Armor;
            leatherArmor.rarity = Rarity.Common;
            leatherArmor.bonusDEF = 5;
            leatherArmor.bonusHP = 10;
            leatherArmor.craftCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 40 }
            };

            // Chainmail
            var chainmail = CreateAsset<EquipmentData>($"{DataPath}/Equipment/Equip_Chainmail.asset");
            chainmail.equipmentId = "chainmail";
            chainmail.displayName = "Chainmail";
            chainmail.description = "Sturdy armor made of interlocking rings.";
            chainmail.slot = EquipmentSlot.Armor;
            chainmail.rarity = Rarity.Rare;
            chainmail.bonusDEF = 15;
            chainmail.bonusHP = 30;
            chainmail.craftCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 300 },
                new ResourceCost { resourceType = ResourceType.Iron, amount = 100 }
            };

            // Lucky Charm
            var luckyCharm = CreateAsset<EquipmentData>($"{DataPath}/Equipment/Equip_LuckyCharm.asset");
            luckyCharm.equipmentId = "lucky_charm";
            luckyCharm.displayName = "Lucky Charm";
            luckyCharm.description = "A mysterious amulet that brings fortune.";
            luckyCharm.slot = EquipmentSlot.Accessory;
            luckyCharm.rarity = Rarity.Uncommon;
            luckyCharm.bonusSPD = 5;
            luckyCharm.craftCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 200 }
            };

            // Database
            var db = CreateAsset<EquipmentDatabase>($"{DataPath}/EquipmentDatabase.asset");
            db.Equipment.Add(ironSword);
            db.Equipment.Add(steelAxe);
            db.Equipment.Add(leatherArmor);
            db.Equipment.Add(chainmail);
            db.Equipment.Add(luckyCharm);

            Debug.Log("[SampleData] Created Equipment data");
        }

        private static EnemyData[] CreateEnemyData()
        {
            // Wolf
            var wolf = CreateAsset<EnemyData>($"{DataPath}/Enemies/Enemy_Wolf.asset");
            wolf.enemyId = "wolf";
            wolf.displayName = "Wild Wolf";
            wolf.hp = 30;
            wolf.atk = 8;
            wolf.def = 2;
            wolf.spd = 12;
            wolf.xpReward = 10;

            // Bandit
            var bandit = CreateAsset<EnemyData>($"{DataPath}/Enemies/Enemy_Bandit.asset");
            bandit.enemyId = "bandit";
            bandit.displayName = "Bandit";
            bandit.hp = 50;
            bandit.atk = 12;
            bandit.def = 5;
            bandit.spd = 8;
            bandit.xpReward = 20;

            // Troll
            var troll = CreateAsset<EnemyData>($"{DataPath}/Enemies/Enemy_Troll.asset");
            troll.enemyId = "troll";
            troll.displayName = "Forest Troll";
            troll.hp = 150;
            troll.atk = 20;
            troll.def = 15;
            troll.spd = 5;
            troll.xpReward = 50;

            // Dragon
            var dragon = CreateAsset<EnemyData>($"{DataPath}/Enemies/Enemy_Dragon.asset");
            dragon.enemyId = "dragon";
            dragon.displayName = "Ancient Dragon";
            dragon.hp = 500;
            dragon.atk = 50;
            dragon.def = 30;
            dragon.spd = 10;
            dragon.xpReward = 200;

            // Database
            var db = CreateAsset<EnemyDatabase>($"{DataPath}/EnemyDatabase.asset");
            db.Enemies.Add(wolf);
            db.Enemies.Add(bandit);
            db.Enemies.Add(troll);
            db.Enemies.Add(dragon);

            Debug.Log("[SampleData] Created Enemy data");
            return new[] { wolf, bandit, troll, dragon };
        }

        private static void CreateDungeonData(EnemyData[] enemies)
        {
            var wolf = enemies[0];
            var bandit = enemies[1];
            var troll = enemies[2];
            var dragon = enemies[3];

            // Dark Forest
            var darkForest = CreateAsset<DungeonData>($"{DataPath}/Dungeons/Dungeon_DarkForest.asset");
            darkForest.dungeonId = "dark_forest";
            darkForest.displayName = "Dark Forest";
            darkForest.description = "A mysterious forest filled with wild beasts.";
            darkForest.floorCount = 5;
            darkForest.energyCost = 10;
            darkForest.enemyPool = new EnemyData[] { wolf, bandit };
            darkForest.enemiesPerFloor = 2;
            darkForest.bossEnemy = bandit;
            darkForest.scalingPerFloor = 0.1f;

            // Bandit Camp
            var banditCamp = CreateAsset<DungeonData>($"{DataPath}/Dungeons/Dungeon_BanditCamp.asset");
            banditCamp.dungeonId = "bandit_camp";
            banditCamp.displayName = "Bandit Camp";
            banditCamp.description = "A camp of ruthless bandits. Beware!";
            banditCamp.floorCount = 10;
            banditCamp.energyCost = 15;
            banditCamp.enemyPool = new EnemyData[] { bandit, wolf };
            banditCamp.enemiesPerFloor = 3;
            banditCamp.bossEnemy = troll;
            banditCamp.scalingPerFloor = 0.15f;

            // Troll Cave
            var trollCave = CreateAsset<DungeonData>($"{DataPath}/Dungeons/Dungeon_TrollCave.asset");
            trollCave.dungeonId = "troll_cave";
            trollCave.displayName = "Troll Cave";
            trollCave.description = "Deep caves where trolls dwell.";
            trollCave.floorCount = 15;
            trollCave.energyCost = 20;
            trollCave.enemyPool = new EnemyData[] { troll, bandit };
            trollCave.enemiesPerFloor = 2;
            trollCave.bossEnemy = dragon;
            trollCave.scalingPerFloor = 0.2f;

            // Database
            var db = CreateAsset<DungeonDatabase>($"{DataPath}/DungeonDatabase.asset");
            db.Dungeons.Add(darkForest);
            db.Dungeons.Add(banditCamp);
            db.Dungeons.Add(trollCave);

            Debug.Log("[SampleData] Created Dungeon data");
        }

        private static void CreateFarmData()
        {
            // Wheat
            var wheat = CreateAsset<FarmPlotData>($"{DataPath}/Farm/Crop_Wheat.asset");
            wheat.farmPlotId = "wheat";
            wheat.displayName = "Wheat";
            wheat.description = "Basic crop, grows quickly.";
            wheat.farmType = FarmType.Crop;
            wheat.growTimeSeconds = 60f; // 1 minute for testing
            wheat.yieldResource = ResourceType.Food;
            wheat.yieldAmount = 25;
            wheat.plantCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 10 }
            };

            // Carrots
            var carrots = CreateAsset<FarmPlotData>($"{DataPath}/Farm/Crop_Carrots.asset");
            carrots.farmPlotId = "carrots";
            carrots.displayName = "Carrots";
            carrots.description = "Nutritious vegetables.";
            carrots.farmType = FarmType.Crop;
            carrots.growTimeSeconds = 120f; // 2 minutes
            carrots.yieldResource = ResourceType.Food;
            carrots.yieldAmount = 65;
            carrots.plantCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 25 }
            };

            // Chickens
            var chickens = CreateAsset<FarmPlotData>($"{DataPath}/Farm/Animal_Chickens.asset");
            chickens.farmPlotId = "chickens";
            chickens.displayName = "Chickens";
            chickens.description = "Provide eggs and meat.";
            chickens.farmType = FarmType.Animal;
            chickens.growTimeSeconds = 300f; // 5 minutes
            chickens.yieldResource = ResourceType.Food;
            chickens.yieldAmount = 125;
            chickens.plantCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Gold, amount = 100 },
                new ResourceCost { resourceType = ResourceType.Food, amount = 50 }
            };

            // Database
            var db = CreateAsset<FarmDatabase>($"{DataPath}/FarmDatabase.asset");
            db.farmPlots.Add(wheat);
            db.farmPlots.Add(carrots);
            db.farmPlots.Add(chickens);

            Debug.Log("[SampleData] Created Farm data");
        }

        private static void CreateMilestoneData()
        {
            // First Gold
            var firstGold = CreateAsset<MilestoneData>($"{DataPath}/Progression/Milestone_FirstGold.asset");
            firstGold.milestoneId = "first_gold";
            firstGold.displayName = "First Fortune";
            firstGold.description = "Accumulate 1,000 gold.";
            firstGold.category = "Early";
            firstGold.conditions = new MilestoneCondition[]
            {
                new MilestoneCondition
                {
                    type = MilestoneConditionType.ResourceAmount,
                    resourceType = ResourceType.Gold,
                    requiredValue = 1000
                }
            };
            firstGold.rewards = new MilestoneReward[]
            {
                new MilestoneReward
                {
                    rewardType = MilestoneRewardType.Resource,
                    resourceType = ResourceType.Gold,
                    amount = 500
                }
            };

            // Builder
            var builder = CreateAsset<MilestoneData>($"{DataPath}/Progression/Milestone_Builder.asset");
            builder.milestoneId = "builder";
            builder.displayName = "Master Builder";
            builder.description = "Upgrade buildings 10 times.";
            builder.category = "Early";
            builder.conditions = new MilestoneCondition[]
            {
                new MilestoneCondition
                {
                    type = MilestoneConditionType.BuildingLevel,
                    targetId = "town_hall",
                    requiredValue = 3
                }
            };
            builder.rewards = new MilestoneReward[]
            {
                new MilestoneReward
                {
                    rewardType = MilestoneRewardType.Resource,
                    resourceType = ResourceType.Wood,
                    amount = 500
                }
            };

            // Recruiter
            var recruiter = CreateAsset<MilestoneData>($"{DataPath}/Progression/Milestone_Recruiter.asset");
            recruiter.milestoneId = "recruiter";
            recruiter.displayName = "Recruiter";
            recruiter.description = "Recruit 3 vikings.";
            recruiter.category = "Early";
            recruiter.conditions = new MilestoneCondition[]
            {
                new MilestoneCondition
                {
                    type = MilestoneConditionType.VikingCount,
                    requiredValue = 3
                }
            };
            recruiter.rewards = new MilestoneReward[]
            {
                new MilestoneReward
                {
                    rewardType = MilestoneRewardType.Resource,
                    resourceType = ResourceType.Food,
                    amount = 300
                }
            };

            // Dungeon Diver
            var dungeonDiver = CreateAsset<MilestoneData>($"{DataPath}/Progression/Milestone_DungeonDiver.asset");
            dungeonDiver.milestoneId = "dungeon_diver";
            dungeonDiver.displayName = "Dungeon Diver";
            dungeonDiver.description = "Clear floor 5 of any dungeon.";
            dungeonDiver.category = "Mid";
            dungeonDiver.conditions = new MilestoneCondition[]
            {
                new MilestoneCondition
                {
                    type = MilestoneConditionType.DungeonCleared,
                    targetId = "dark_forest",
                    requiredValue = 1
                }
            };
            dungeonDiver.rewards = new MilestoneReward[]
            {
                new MilestoneReward
                {
                    rewardType = MilestoneRewardType.EnergyCapIncrease,
                    amount = 10
                }
            };

            // Database
            var db = CreateAsset<MilestoneDatabase>($"{DataPath}/MilestoneDatabase.asset");
            db.Milestones.Add(firstGold);
            db.Milestones.Add(builder);
            db.Milestones.Add(recruiter);
            db.Milestones.Add(dungeonDiver);

            Debug.Log("[SampleData] Created Milestone data");
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
#endif
