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
                    // Level 1: Hellpuppy-only rooms.
                    // Range: 3-6.
                    new EncounterTemplate("L1_Hellpuppies_3", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L1_Hellpuppies_4", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L1_Hellpuppies_5", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L1_Hellpuppies_6", new EnemyType[]
                    {
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    })
                };

            case 2:
                return new EncounterTemplate[]
                {
                    // Level 2: Vermin is introduced.
                    // Vermin-only range: 2-5.
                    // No Hellpuppy-only rooms.
                    new EncounterTemplate("L2_Vermin_2", new EnemyType[]
                    {
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L2_Vermin_3", new EnemyType[]
                    {
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L2_Vermin_4", new EnemyType[]
                    {
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L2_Vermin_5", new EnemyType[]
                    {
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L2_Vermin_Hellpuppy_A", new EnemyType[]
                    {
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L2_Vermin_Hellpuppy_B", new EnemyType[]
                    {
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L2_Vermin_Hellpuppy_C", new EnemyType[]
                    {
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L2_Vermin_Hellpuppy_D", new EnemyType[]
                    {
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    })
                };

            case 3:
                return new EncounterTemplate[]
                {
                    // Level 3: Inferno is introduced.
                    // Can be Inferno-only, Inferno + Vermin, Inferno + Hellpuppy,
                    // or Inferno + Vermin + Hellpuppy.
                    new EncounterTemplate("L3_Inferno_2", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Inferno
                    }),
                    new EncounterTemplate("L3_Inferno_3", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Inferno
                    }),
                    new EncounterTemplate("L3_Inferno_4", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Inferno
                    }),
                    new EncounterTemplate("L3_Inferno_5", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Inferno
                    }),
                    new EncounterTemplate("L3_Inferno_Vermin_A", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L3_Inferno_Vermin_B", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L3_Inferno_Hellpuppy_A", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L3_Inferno_Hellpuppy_B", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L3_Inferno_Mixed_A", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L3_Inferno_Mixed_B", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L3_Inferno_Mixed_C", new EnemyType[]
                    {
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    })
                };

            case 4:
                return new EncounterTemplate[]
                {
                    // Level 4: Warden is introduced.
                    // Wardens should make the player stop brainless shooting.
                    new EncounterTemplate("L4_Warden_Intro_A", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L4_Warden_Intro_B", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L4_Warden_Inferno_A", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L4_Warden_Inferno_B", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L4_Warden_Mixed_A", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L4_Warden_Mixed_B", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L4_Warden_Mixed_C", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno
                    }),
                    new EncounterTemplate("L4_Warden_Mixed_D", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    })
                };

            case 5:
                return new EncounterTemplate[]
                {
                    // Level 5: Devil's Advocate is introduced.
                    // No solo Devil's Advocate. Always protect him with pressure.
                    new EncounterTemplate("L5_Advocate_Intro_A", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Hellpuppy,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L5_Advocate_Intro_B", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L5_Advocate_Inferno_A", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Inferno,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L5_Advocate_Inferno_B", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Inferno,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L5_Advocate_Warden_A", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L5_Advocate_Warden_B", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L5_Advocate_Mixed_A", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L5_Advocate_Mixed_B", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    })
                };

            case 6:
                return new EncounterTemplate[]
                {
                    // Level 6: all types can appear.
                    // Still some Hellpuppies, but no longer as the main meal.
                    new EncounterTemplate("L6_Mixed_A", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L6_Mixed_B", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L6_Mixed_C", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L6_Mixed_D", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L6_Mixed_E", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L6_Mixed_F", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    })
                };

            case 7:
                return new EncounterTemplate[]
                {
                    // Level 7: 6-7 enemies, more high-type pressure.
                    new EncounterTemplate("L7_Elite_A", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L7_Elite_B", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L7_Elite_C", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L7_Elite_D", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L7_Elite_E", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L7_Elite_F", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    })
                };

            case 8:
                return new EncounterTemplate[]
                {
                    // Level 8: 7-8 enemies. Hellpuppies become rare.
                    new EncounterTemplate("L8_DeepHell_A", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L8_DeepHell_B", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L8_DeepHell_C", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L8_DeepHell_D", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Hellpuppy
                    }),
                    new EncounterTemplate("L8_DeepHell_E", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L8_DeepHell_F", new EnemyType[]
                    {
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    })
                };

            case 9:
                return new EncounterTemplate[]
                {
                    // Level 9: brutal rooms. Strong lean toward Wardens / Devil's Advocates.
                    new EncounterTemplate("L9_Nightmare_A", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L9_Nightmare_B", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L9_Nightmare_C", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L9_Nightmare_D", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L9_Nightmare_E", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin
                    })
                };

            case 10:
                return new EncounterTemplate[]
                {
                    // Level 10: final normal-room nightmare tier.
                    // High types dominate. Hellpuppies are basically gone.
                    new EncounterTemplate("L10_Abyss_A", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L10_Abyss_B", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L10_Abyss_C", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno
                    }),
                    new EncounterTemplate("L10_Abyss_D", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin
                    }),
                    new EncounterTemplate("L10_Abyss_E", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin
                    })
                };

            default:
                // If combat level goes beyond 10, keep using level 10-style rooms.
                return new EncounterTemplate[]
                {
                    new EncounterTemplate("Fallback_Abyss", new EnemyType[]
                    {
                        EnemyType.DevilsAdvocate,
                        EnemyType.DevilsAdvocate,
                        EnemyType.Warden,
                        EnemyType.Warden,
                        EnemyType.Inferno,
                        EnemyType.Inferno,
                        EnemyType.Vermin,
                        EnemyType.Vermin
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