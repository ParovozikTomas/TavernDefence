'use server';

/**
 * @fileOverview An AI agent for controlling hero skills reactively during gameplay.
 *
 * - adjustHeroSkills - A function that determines when and how heroes should use their skills.
 * - AdjustHeroSkillsInput - The input type for the adjustHeroSkills function.
 * - AdjustHeroSkillsOutput - The return type for the adjustHeroSkills function.
 */

import {ai} from '@/ai/genkit';
import {z} from 'genkit';

const AdjustHeroSkillsInputSchema = z.object({
  gameState: z
    .string()
    .describe(
      'A string representing the current game state, including hero positions, health, enemy positions, and available skills.'
    ),
});
export type AdjustHeroSkillsInput = z.infer<typeof AdjustHeroSkillsInputSchema>;

const AdjustHeroSkillsOutputSchema = z.object({
  skillActivations: z.array(
    z.object({
      heroId: z.string().describe('The ID of the hero using the skill.'),
      skillName: z.string().describe('The name of the skill to activate.'),
      targetArea: z
        .string()
        .describe(
          'Description of the target area for the skill (e.g., coordinates or area description).'
        ),
      reason: z
        .string()
        .describe('Reasoning for the skill usage based on the game state'),
    })
  ).describe('A list of skills to activate with their target areas and reasoning.'),
});
export type AdjustHeroSkillsOutput = z.infer<typeof AdjustHeroSkillsOutputSchema>;

export async function adjustHeroSkills(input: AdjustHeroSkillsInput): Promise<AdjustHeroSkillsOutput> {
  return adjustHeroSkillsFlow(input);
}

const prompt = ai.definePrompt({
  name: 'adjustHeroSkillsPrompt',
  input: {schema: AdjustHeroSkillsInputSchema},
  output: {schema: AdjustHeroSkillsOutputSchema},
  prompt: `You are an AI battle strategist for the Tavern Guardians game. Analyze the current game state and determine the optimal skill activations for the heroes.

Game State:
{{{gameState}}}

Based on the game state, determine which skills should be activated, considering factors like enemy positions, hero health, and skill effects. Provide a detailed reasoning for each skill activation.

Output a JSON array of skill activations, including the hero ID, skill name, target area, and reasoning. Prioritize skills that maximize damage, protect heroes, or control the battlefield.

Example:
[
  {
    "heroId": "archer1",
    "skillName": "Rain of Arrows",
    "targetArea": "Area with a large group of enemies near the tavern",
    "reason": "Rain of Arrows will deal significant damage to the clustered enemies, protecting the tavern."
  },
  {
    "heroId": "mage1",
    "skillName": "Healing Field",
    "targetArea": "Area around the warrior who is taking heavy damage",
    "reason": "The warrior is low on health and surrounded by enemies. Healing Field will keep him alive and allow him to continue tanking."
  }
]

Ensure the output is a valid JSON array.
`,
});

const adjustHeroSkillsFlow = ai.defineFlow(
  {
    name: 'adjustHeroSkillsFlow',
    inputSchema: AdjustHeroSkillsInputSchema,
    outputSchema: AdjustHeroSkillsOutputSchema,
  },
  async input => {
    const {output} = await prompt(input);
    return output!;
  }
);
