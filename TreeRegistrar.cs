using System.Collections.Generic;
using Jotunn.Configs;

namespace Ravenwood.Biomes
{
    public sealed class TreeRegistration
    {
        public string PrefabName;
        public string DisplayName;
        public RequirementConfig[] Requirements;
        public string Description;
        public string Category;
        public string DefaultDropItem;
        public int DefaultDropMin;
        public int DefaultDropMax;
        public bool DefaultEnableDrops;
        public string SeedPrefabName;
        public int DefaultSeedDropMin;
        public int DefaultSeedDropMax;
        public float DefaultSeedDropChance;
        public bool DefaultEnableSeedDrops;
        public bool DefaultEnableRespawn;
        public float DefaultRespawnMinutes;
        public bool DefaultIndestructible;

        public TreeRegistration(
            string prefabName,
            string displayName,
            RequirementConfig[] requirements,
            string description,
            string category,
            string defaultDropItem,
            int defaultDropMin,
            int defaultDropMax,
            bool defaultEnableDrops,
            string seedPrefabName,
            int defaultSeedDropMin,
            int defaultSeedDropMax,
            float defaultSeedDropChance,
            bool defaultEnableSeedDrops,
            bool defaultEnableRespawn,
            float defaultRespawnMinutes,
            bool defaultIndestructible)
        {
            PrefabName = prefabName;
            DisplayName = displayName;
            Requirements = requirements;
            Description = description;
            Category = category;
            DefaultDropItem = defaultDropItem;
            DefaultDropMin = defaultDropMin;
            DefaultDropMax = defaultDropMax;
            DefaultEnableDrops = defaultEnableDrops;
            SeedPrefabName = seedPrefabName;
            DefaultSeedDropMin = defaultSeedDropMin;
            DefaultSeedDropMax = defaultSeedDropMax;
            DefaultSeedDropChance = defaultSeedDropChance;
            DefaultEnableSeedDrops = defaultEnableSeedDrops;
            DefaultEnableRespawn = defaultEnableRespawn;
            DefaultRespawnMinutes = defaultRespawnMinutes;
            DefaultIndestructible = defaultIndestructible;
        }
    }

    public static class TreeRegistrar
    {
        public const string RavenSeedPrefabName = "RWB_Ravenwood_Seed";
        public const string RavenSeedDisplayName = "Ravenwood Seed";
        public const string RavenElixirPrefabName = "RWB_Ravenwood_Elixir";
        public const string RavenElixirDisplayName = "Ravenwood Elixir";
        public const string RavenSerumPrefabName = "RWB_Ravenwood_Serum";
        public const string RavenSerumDisplayName = "Ravenwood Serum";
        public const string PurpleMushroomPrefabName = "RWB_Pickable_Purple_Mushroom";
        public const string PurpleMushroomDisplayName = "Ravenwood Mushroom";
        public const string PurpleMushroomPickableDisplayName = "Pickable Ravenwood Mushrooms";
        public const string PurpleMushroomItemPrefabName = "RWB_Purple_Mushroom";
        public const string GreenMushroomPrefabName = "RWB_Pickable_Green_Mushroom";
        public const string GreenMushroomDisplayName = "Green Mushroom";
        public const string GreenMushroomPickableDisplayName = "Pickable Green Mushrooms";
        public const string GreenMushroomItemPrefabName = "RWB_Green_Mushroom";
        public const float DefaultSeedDropChance = 0.25f;

        private const string DefaultDescription = "Plant a Ravenwood Vegetation with the cultivator.";
        private const int DefaultSeedDropMin = 1;
        private const int DefaultSeedDropMax = 2;

        public static readonly List<TreeRegistration> AllRegistrations = new List<TreeRegistration>
        {
//STANDARD
            //<<<<<<<< TREES >>>>>>>

            CreateDefaultRegistration(
                //01
                prefabName: "RWB_Tree1", displayName: "Ravenwood Tree I",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropMin: 5, defaultDropMax: 10),

            CreateDefaultRegistration(
                //02
                prefabName: "RWB_Tree2", displayName: "Ravenwood Tree II",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropMin: 5, defaultDropMax: 10),

            CreateDefaultRegistration(
                //03
                prefabName: "RWB_Tree3", displayName: "Ravenwood Tree III",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropMin: 5, defaultDropMax: 10),

            CreateDefaultRegistration(
                //04
                prefabName: "RWB_Tree4", displayName: "Ravenwood Tree IV",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropMin: 5, defaultDropMax: 10),

            CreateDefaultRegistration(
                //05
                prefabName: "RWB_Tree5", displayName: "Ravenwood Tree V",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropMin: 5, defaultDropMax: 10),

            CreateDefaultRegistration(
                //06
                prefabName: "RWB_Tree6", displayName: "Ravenwood Tree VI",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropMin: 5, defaultDropMax: 10),

            CreateDefaultRegistration(
                //07
                prefabName: "RWB_Tree7", displayName: "Ravenwood Tree VII",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropMin: 5, defaultDropMax: 10),

            CreateDefaultRegistration(
                //08
                prefabName: "RWB_Tree8", displayName: "Ravenwood Tree VIII",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropMin: 5, defaultDropMax: 10),

            CreateDefaultRegistration(
                //09
                prefabName: "RWB_Tree9", displayName: "Ravenwood Tree IX",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropMin: 5, defaultDropMax: 10),

            CreateDefaultRegistration(
                //07
                prefabName: "RWB_Tree10", displayName: "Ravenwood Tree X.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropMin: 5, defaultDropMax: 10),

            CreateDefaultRegistration(
                //13
                prefabName: "RWB_Tree11", displayName: "Ravenwood Tree XI.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropMin: 5, defaultDropMax: 10),

            //<<<<<<<< MUSHROOMS >>>>>>>

            CreateMushroomRegistration(
                //03
                prefabName: GreenMushroomPrefabName, displayName: GreenMushroomPickableDisplayName,
                requirementItem: GreenMushroomItemPrefabName,
                defaultDropItem: GreenMushroomItemPrefabName),

            CreateMushroomRegistration(
                //05
                prefabName: PurpleMushroomPrefabName, displayName: PurpleMushroomPickableDisplayName,
                requirementItem: PurpleMushroomItemPrefabName,
                defaultDropItem: PurpleMushroomItemPrefabName),

            CreateDefaultRegistration(
                //10
                prefabName: "RWB_Mushroom1", displayName: "Ravenwood Mushroom I.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateDefaultRegistration(
                //11
                prefabName: "RWB_Mushroom2", displayName: "Ravenwood Mushroom II.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateDefaultRegistration(
                //12
                prefabName: "RWB_Mushroom3", displayName: "Ravenwood Mushroom III.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateDefaultRegistration(
                //10
                prefabName: "RWB_Mushroom4", displayName: "Ravenwood Mushroom IV.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateDefaultRegistration(
                //11
                prefabName: "RWB_Mushroom5", displayName: "Ravenwood Mushroom V.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateDefaultRegistration(
                //12
                prefabName: "RWB_Mushroom6", displayName: "Ravenwood Mushroom VI.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateDefaultRegistration(
                //18
                prefabName: "RWB_Mushroom7", displayName: "Ravenwood Mushroom VII.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateDefaultRegistration(
                //18
                prefabName: "RWB_Mushroom8", displayName: "Ravenwood Mushroom VIII.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            //<<<<<<<< FUNGI >>>>>>>

            CreateDefaultRegistration(
                //01
                prefabName: "RWB_Fungi1", displayName: "Ravenwood Fungi I.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 2),

            CreateDefaultRegistration(
                //02
                prefabName: "RWB_Fungi2", displayName: "Ravenwood Fungi II.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 2),

            CreateDefaultRegistration(
                //06
                prefabName: "RWB_Fungi3", displayName: "Ravenwood Fungi III.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 2),

            CreateDefaultRegistration(
                //07
                prefabName: "RWB_Fungi4", displayName: "Ravenwood Fungi IV.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 2),

            CreateDefaultRegistration(
                //08
                prefabName: "RWB_Fungi5", displayName: "Ravenwood Fungi V.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 2),

            CreateDefaultRegistration(
                //09
                prefabName: "RWB_Fungi6", displayName: "Ravenwood Fungi VI.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 2),

                        CreateDefaultRegistration(
                //10
                prefabName: "RWB_Fungi7", displayName: "Ravenwood Fungi VII.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 2),

            //<<<<<<<< PLANTS >>>>>>>

            CreateDefaultRegistration(
                //01
                prefabName: "RWB_Plant1", displayName: "Ravenwood Plant I.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //02
                prefabName: "RWB_Plant2", displayName: "Ravenwood Plant II.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //03
                prefabName: "RWB_Plant3", displayName: "Ravenwood Plant III.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //04
                prefabName: "RWB_Plant4", displayName: "Ravenwood Plant IV.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //05
                prefabName: "RWB_Plant5", displayName: "Ravenwood Plant V.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //06
                prefabName: "RWB_Plant6", displayName: "Ravenwood Plant VI.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //03
                prefabName: "RWB_Plant7", displayName: "Ravenwood Plant VII.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //04
                prefabName: "RWB_Plant8", displayName: "Ravenwood Plant VIII.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //05
                prefabName: "RWB_Plant9", displayName: "Ravenwood Plant IX.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //09
                prefabName: "RWB_Plant10", displayName: "Ravenwood Plant X.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //13
                prefabName: "RWB_Plant11", displayName: "Ravenwood Plant XI.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //14
                prefabName: "RWB_Plant12", displayName: "Ravenwood Plant XII.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //15
                prefabName: "RWB_Plant13", displayName: "Ravenwood Plant XIII.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //16
                prefabName: "RWB_Plant14", displayName: "Ravenwood Plant XIV.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //17
                prefabName: "RWB_Plant15", displayName: "Ravenwood Plant XV.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

            CreateDefaultRegistration(
                //18
                prefabName: "RWB_Plant16", displayName: "Ravenwood Plant XVI.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

                        CreateDefaultRegistration(
                //18
                prefabName: "RWB_Plant17", displayName: "Ravenwood Plant XVII.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 1),

                        //<<<<<<<< Grass >>>>>>>

            CreateDefaultRegistration(
               //01
                prefabName: "RWB_Grass1", displayName: "Ravenwood Grass.",
                category: RavenwoodBiomes.GetPreferredCategoryName(),
                defaultDropItem: "Dandelion",
                defaultDropMin: 1, defaultDropMax: 1),

//SCALED

           
            //<<<<<<<< SCALED TREES >>>>>>>

            CreateScaledRegistration(
                //01
                prefabName: "RWB_ScaledTree1", displayName: "Ravenwood Scaled Tree I.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropMin: 10, defaultDropMax: 20),

             CreateScaledRegistration(
                //02
                prefabName: "RWB_ScaledTree2", displayName: "Ravenwood Scaled Tree II.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropMin: 10, defaultDropMax: 20),

              CreateScaledRegistration(
                //03
                prefabName: "RWB_ScaledTree3", displayName: "Ravenwood Scaled Tree III.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropMin: 10, defaultDropMax: 20),

               CreateScaledRegistration(
                //04
                prefabName: "RWB_ScaledTree4", displayName: "Ravenwood Scaled Tree IV.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropMin: 10, defaultDropMax: 20),

               CreateScaledRegistration(
                //05
                prefabName: "RWB_ScaledTree6", displayName: "Ravenwood Scaled Tree VI.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropMin: 10, defaultDropMax: 20),

                 CreateScaledRegistration(
                //06
                prefabName: "RWB_ScaledTree7", displayName: "Ravenwood Scaled Tree VII.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropMin: 10, defaultDropMax: 20),

                CreateScaledRegistration(
                //07
                prefabName: "RWB_ScaledTree10", displayName: "Ravenwood Scaled Tree X.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropMin: 10, defaultDropMax: 20),

            CreateScaledRegistration(
                //08
                prefabName: "RWB_ScaledTree11", displayName: "Ravenwood Scaled Tree XI.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropMin: 10, defaultDropMax: 20),

            CreateScaledRegistration(
                //09
                prefabName: "RWB_ScaledTree11_purple", displayName: "Ravenwood Scaled Tree XI Purple.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropMin: 10, defaultDropMax: 20),
                                    
            //<<<<<<<< SCALED MUSHROOMS >>>>>>>
                     

                CreateScaledRegistration(
                //01
                prefabName: "RWB_ScaledMushroom1", displayName: "Ravenwood Scaled Mushroom I.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 2, defaultDropMax: 5),

                CreateScaledRegistration(
                //02
                prefabName: "RWB_ScaledMushroom2", displayName: "Ravenwood Scaled Mushroom VII.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 2, defaultDropMax: 5),

                CreateScaledRegistration(
                //03
                prefabName: "RWB_ScaledMushroom3", displayName: "Ravenwood Scaled Mushroom III.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 2, defaultDropMax: 5),

                CreateScaledRegistration(
                //04
                prefabName: "RWB_ScaledMushroom4", displayName: "Ravenwood Scaled Mushroom IV.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 2, defaultDropMax: 5),

                CreateScaledRegistration(
                //05
                prefabName: "RWB_ScaledMushroom7", displayName: "Ravenwood Scaled Mushroom VII.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 2, defaultDropMax: 5),

                            CreateScaledRegistration(
                //06
                prefabName: "RWB_ScaledMushroom8", displayName: "Ravenwood Scaled Mushroom VIII.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),
                
                //<<<<<<<< SCALED FUNGI >>>>>>>
            
            CreateScaledRegistration(
                //01
                prefabName: "RWB_ScaledFungi1", displayName: "Ravenwood Scaled Fungi I.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 2, defaultDropMax: 5),

            CreateScaledRegistration(
                //02
                prefabName: "RWB_ScaledFungi3", displayName: "Ravenwood Scaled Fungi III.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 2, defaultDropMax: 5),

                      CreateScaledRegistration(
                //03
                prefabName: "RWB_ScaledFungi4", displayName: "Ravenwood Scaled Fungi IV.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 2, defaultDropMax: 5),

             CreateScaledRegistration(
                //04
                prefabName: "RWB_ScaledFungi6", displayName: "Ravenwood Scaled Fungi VI.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 2, defaultDropMax: 5),

             CreateScaledRegistration(
                //05
                prefabName: "RWB_ScaledFungi7", displayName: "Ravenwood Scaled Fungi VII.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 2, defaultDropMax: 5),



            //<<<<<<<< SCALED PLANTS >>>>>>>

            CreateScaledRegistration(
                //01
                prefabName: "RWB_ScaledPlant1", displayName: "Ravenwood Scaled Plant I.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateScaledRegistration(
                //02
                prefabName: "RWB_ScaledPlant2", displayName: "Ravenwood Scaled Plant II.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

                //03

            CreateScaledRegistration(
                //04
                prefabName: "RWB_ScaledPlant4", displayName: "Ravenwood Scaled Plant IV.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateScaledRegistration(
                //05
                prefabName: "RWB_ScaledPlant7", displayName: "Ravenwood Scaled Plant VII.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateScaledRegistration(
                //06
                prefabName: "RWB_ScaledPlant8", displayName: "Ravenwood Scaled Plant VIII.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateScaledRegistration(
                //07
                prefabName: "RWB_ScaledPlant9", displayName: "Ravenwood Scaled Plant IX.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateScaledRegistration(
                //08
                prefabName: "RWB_ScaledPlant10", displayName: "Ravenwood Scaled Plant X.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateScaledRegistration(
                //09
                prefabName: "RWB_ScaledPlant11", displayName: "Ravenwood Scaled Plant XI.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateScaledRegistration(
                //10
                prefabName: "RWB_ScaledPlant12", displayName: "Ravenwood Scaled Plant XII.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateScaledRegistration(
                //11
                prefabName: "RWB_ScaledPlant13", displayName: "Ravenwood Scaled Plant XIII.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateScaledRegistration(
                //12
                prefabName: "RWB_ScaledPlant14", displayName: "Ravenwood Scaled Plant XIV.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateScaledRegistration(
                //13
                prefabName: "RWB_ScaledPlant15", displayName: "Ravenwood Scaled Plant XV.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

            CreateScaledRegistration(
                //14
                prefabName: "RWB_ScaledPlant16", displayName: "Ravenwood Scaled Plant XVI.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),

                        CreateScaledRegistration(
                //15
                prefabName: "RWB_ScaledPlant17", displayName: "Ravenwood Scaled Plant XVII.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Mushroom",
                defaultDropMin: 1, defaultDropMax: 3),


           //<<<<<<<< Scaled Grass >>>>>>>

            CreateScaledRegistration(
               //01
                prefabName: "RWB_ScaledGrass1", displayName: "Ravenwood Tall Grass.",
                category: RavenwoodBiomes.GetScaledCategoryName(),
                defaultDropItem: "Dandelion",
                defaultDropMin: 1, defaultDropMax: 3),


//ETERNAL
            //<<<<<<<< ETERNAL TREES >>>>>>>

            CreateEternalRegistration(
                //01
                prefabName: "RWB_Eternal_ScaledTree11", displayName: "Ravenwood Eternal Scaled Tree XI."),

            CreateEternalRegistration(
                //02
                prefabName: "RWB_Eternal_ScaledTree11_purple", displayName: "Ravenwood Eternal Scaled Tree XI purple."),

        };

        private static TreeRegistration CreateMushroomRegistration(
            string prefabName,
            string displayName,
            string requirementItem,
            string defaultDropItem)
        {
            return new TreeRegistration(
                prefabName: prefabName,
                displayName: displayName,
                requirements: CreateMushroomRequirement(requirementItem),
                description: DefaultDescription,
                category: "Misc",
                defaultDropItem: defaultDropItem,
                defaultDropMin: 1,
                defaultDropMax: 1,
                defaultEnableDrops: true,
                seedPrefabName: requirementItem,
                defaultSeedDropMin: 0,
                defaultSeedDropMax: 0,
                defaultSeedDropChance: 0f,
                defaultEnableSeedDrops: false,
                defaultEnableRespawn: false,
                defaultRespawnMinutes: 0f,
                defaultIndestructible: false);
        }

        private static TreeRegistration CreateDefaultRegistration(
            string prefabName,
            string displayName,
            string category,
            int defaultDropMin,
            int defaultDropMax,
            string defaultDropItem = "Wood")
        {
            return new TreeRegistration(
                prefabName: prefabName,
                displayName: displayName,
                requirements: CreateSeedRequirement(),
                description: DefaultDescription,
                category: category,
                defaultDropItem: defaultDropItem,
                defaultDropMin: defaultDropMin,
                defaultDropMax: defaultDropMax,
                defaultEnableDrops: true,
                seedPrefabName: RavenSeedPrefabName,
                defaultSeedDropMin: DefaultSeedDropMin,
                defaultSeedDropMax: DefaultSeedDropMax,
                defaultSeedDropChance: DefaultSeedDropChance,
                defaultEnableSeedDrops: true,
                defaultEnableRespawn: false,
                defaultRespawnMinutes: 0f,
                defaultIndestructible: false);
        }

        private static TreeRegistration CreateScaledRegistration(
            string prefabName,
            string displayName,
            string category,
            int defaultDropMin,
            int defaultDropMax,
            string defaultDropItem = "Wood")
        {
            return new TreeRegistration(
                prefabName: prefabName,
                displayName: displayName,
                requirements: CreateScaledRequirement(),
                description: DefaultDescription,
                category: category,
                defaultDropItem: defaultDropItem,
                defaultDropMin: defaultDropMin,
                defaultDropMax: defaultDropMax,
                defaultEnableDrops: true,
                seedPrefabName: RavenSeedPrefabName,
                defaultSeedDropMin: DefaultSeedDropMin,
                defaultSeedDropMax: DefaultSeedDropMax,
                defaultSeedDropChance: DefaultSeedDropChance,
                defaultEnableSeedDrops: true,
                defaultEnableRespawn: false,
                defaultRespawnMinutes: 0f,
                defaultIndestructible: false);
        }

        private static TreeRegistration CreateEternalRegistration(
            string prefabName,
            string displayName)
        {
            return new TreeRegistration(
                prefabName: prefabName,
                displayName: displayName,
                requirements: CreateEternalRequirement(),
                description: DefaultDescription,
                category: RavenwoodBiomes.GetEternalCategoryName(),
                defaultDropItem: string.Empty,
                defaultDropMin: 0,
                defaultDropMax: 0,
                defaultEnableDrops: false,
                seedPrefabName: string.Empty,
                defaultSeedDropMin: 0,
                defaultSeedDropMax: 0,
                defaultSeedDropChance: 0f,
                defaultEnableSeedDrops: false,
                defaultEnableRespawn: false,
                defaultRespawnMinutes: 0f,
                defaultIndestructible: true);
        }

        private static RequirementConfig[] CreateMushroomRequirement(string requirementItem)
        {
            return new[]
            {
                new RequirementConfig(requirementItem, 100, 0, true)
            };
        }

        private static RequirementConfig[] CreateSeedRequirement()
        {
            return new[]
            {
                new RequirementConfig(RavenSeedPrefabName, 1, 0, true)
            };
        }

        private static RequirementConfig[] CreateScaledRequirement()
        {
            return new[]
            {
                new RequirementConfig(RavenSeedPrefabName, 1, 0, true),
                new RequirementConfig(RavenSerumPrefabName, 1, 0, true)
            };
        }

        private static RequirementConfig[] CreateEternalRequirement()
        {
            return new[]
            {
                new RequirementConfig(RavenSeedPrefabName, 1, 0, true),
                new RequirementConfig(RavenElixirPrefabName, 1, 0, true)
            };
        }

        public static IEnumerable<string> GetAllCategories()
        {
            yield return RavenwoodBiomes.GetPreferredCategoryName();
            yield return RavenwoodBiomes.GetScaledCategoryName();
            yield return RavenwoodBiomes.GetEternalCategoryName();
        }
    }
}