using System.Collections.Generic;
using UnityEngine;

public static class EncounterGenerator
{
    private struct EncounterTemplate
    {
        public string Id;
        public EnemyType[] Enemies;

        public EncounterTemplate(string id, EnemyType[] enemies)
        {
            Id = id;
            Enemies = enemies;
        }
    }

    public static List<RoomEnemyStateEntry> Generate(
        Vector2Int roomCoord,
        int combatLevel,
        int encounterSeed,
        int spawnPointCount)
    {
        var result = new List<RoomEnemyStateEntry>();

        if (spawnPointCount <= 0)
            return result;

        EncounterTemplate[] templates = GetTemplatesForCombatLevel(combatLevel);
        EncounterTemplate chosenTemplate = PickTemplate(templates, encounterSeed);

        int enemyCount = Mathf.Clamp(chosenTemplate.Enemies.Length, 1, spawnPointCount);
        List<int> shuffledSpawnIndices = BuildShuffledIndices(spawnPointCount, encounterSeed);

        for (int i = 0; i < enemyCount; i++)
        {
            result.Add(new RoomEnemyStateEntry(
                chosenTemplate.Enemies[i],
                shuffledSpawnIndices[i],
                true
            ));
        }

        return result;
    }

    private static EncounterTemplate[] GetTemplatesForCombatLevel(int combatLevel)
    {
        switch (combatLevel)
        {
            case 1:
                return new EncounterTemplate[]
                {
                    new EncounterTemplate("L1_A", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L1_B", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L1_C", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L1_D", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    })
                };

            case 2:
                return new EncounterTemplate[]
                {
                    new EncounterTemplate("L2_A", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L2_B", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L2_C", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L2_D", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L2_E", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    })
                };

            case 3:
                return new EncounterTemplate[]
                {
                    new EncounterTemplate("L3_A", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Inferno
                    }),
                    new EncounterTemplate("L3_B", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Vermin,
                        EnemyType.Inferno
                    }),
                    new EncounterTemplate("L3_C", new EnemyType[]
                    {
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Inferno
                    }),
                    new EncounterTemplate("L3_D", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Vermin,
                        EnemyType.Inferno
                    }),
                    new EncounterTemplate("L3_E", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Inferno
                    }),
                    new EncounterTemplate("L3_F", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Vermin
                    })
                };

            case 4:
                return new EncounterTemplate[]
                {
                    new EncounterTemplate("L4_A", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L4_B", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L4_C", new EnemyType[]
                    {
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Warden
                    }),
                    new EncounterTemplate("L4_D", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Inferno
                    })
                };   

            case 5:
                return new EncounterTemplate[]
                {
                    new EncounterTemplate("L5_A", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Hellpuppy
                    })
                };

                case 6:
                return new EncounterTemplate[]
                {
                    new EncounterTemplate("L6_A", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate
                    })
                };           

            default:
                return new EncounterTemplate[]
                {
                    new EncounterTemplate("Fallback", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    })
                };
        }
    }

    private static EncounterTemplate PickTemplate(
        EncounterTemplate[] templates,
        int encounterSeed)
    {
        if (templates == null || templates.Length == 0)
        {
            return new EncounterTemplate("Fallback", new EnemyType[]
            {
                EnemyType.Hellpuppy,
                EnemyType.Hellpuppy
            });
        }

        int index = PositiveMod(encounterSeed, templates.Length);
        return templates[index];
    }

    private static List<int> BuildShuffledIndices(int count, int seed)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < count; i++)
            indices.Add(i);

        System.Random rng = new System.Random(seed);

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int temp = indices[i];
            indices[i] = indices[j];
            indices[j] = temp;
        }

        return indices;
    }

    private static int PositiveMod(int value, int mod)
    {
        if (mod <= 0)
            return 0;

        int result = value % mod;
        return result < 0 ? result + mod : result;
    }

    public static int BuildEncounterSeed(Vector2Int coord, int combatLevel)
    {
        unchecked
        {
            int seed = 17;
            seed = seed * 31 + coord.x;
            seed = seed * 31 + coord.y;
            seed = seed * 31 + combatLevel;
            return seed;
        }
    }
}