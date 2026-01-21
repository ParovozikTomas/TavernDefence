'use client';
import { useState } from 'react';
import type { Hero } from '@/lib/game-types';
import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

interface UpgradeShopProps {
  heroes: Hero[];
  gold: number;
  onUpgradesComplete: (updatedHeroes: Hero[], spentGold: number) => void;
}

export default function UpgradeShop({ heroes, gold, onUpgradesComplete }: UpgradeShopProps) {
  const [localHeroes, setLocalHeroes] = useState(heroes);
  const [spentGold, setSpentGold] = useState(0);

  const calculateLevelUpCost = (level: number) => {
    return 50 + (level * 25);
  };

  const handleLevelUp = (heroId: string) => {
    const hero = localHeroes.find(h => h.id === heroId);
    if (!hero) return;

    const cost = calculateLevelUpCost(hero.level);
    if (gold - spentGold < cost) {
      console.warn(`Player action failed: Not enough gold to level up hero ${heroId}.`);
      return;
    }

    console.log(`Player action: Level up for hero ${heroId}. Cost: ${cost} gold.`);
    setSpentGold(prev => prev + cost);
    setLocalHeroes(currentHeroes => currentHeroes.map(h => {
      if (h.id === heroId) {
        return {
          ...h,
          level: h.level + 1,
          maxHealth: h.maxHealth + 20,
          attackDamage: h.attackDamage + 5,
        };
      }
      return h;
    }));
  };

  const handleFinishUpgrading = () => {
    console.log(`Player action: Finished upgrading. Total spent: ${spentGold} gold.`);
    onUpgradesComplete(localHeroes, spentGold);
  };

  return (
    <div className="absolute inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-40 p-4">
      <Card className="w-full max-w-4xl font-headline bg-card/90">
        <CardHeader>
          <CardTitle className="text-4xl text-center">Upgrade Your Guardians</CardTitle>
          <CardDescription className="text-center">Spend your gold to prepare for the next day. Current Gold: {gold - spentGold}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4 max-h-[50vh] overflow-y-auto p-4">
          {localHeroes.map(hero => {
            if (hero.isDead) {
              return (
                <Card key={hero.id} className="p-4 flex justify-between items-center bg-muted/50">
                  <div>
                    <h4 className="font-bold text-lg text-muted-foreground">{hero.heroClass} (Lvl {hero.level})</h4>
                  </div>
                  <div className="text-destructive font-bold text-xl">DEAD</div>
                </Card>
              );
            }
            
            const cost = calculateLevelUpCost(hero.level);

            return (
              <Card key={hero.id} className="p-4 flex justify-between items-center">
                <div>
                  <h4 className="font-bold text-lg">{hero.heroClass} (Lvl {hero.level})</h4>
                  <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm text-muted-foreground mt-2">
                    <span><span role="img" aria-label="Health">❤️</span> HP: {hero.maxHealth}</span>
                    <span><span role="img" aria-label="Damage">⚔️</span> Dmg: {hero.attackDamage}</span>
                    <span><span role="img" aria-label="Attack Speed">⚡</span> Spd: {hero.attackSpeed.toFixed(2)}</span>
                    <span><span role="img" aria-label="Range">🏹</span> Rng: {hero.range}</span>
                  </div>
                </div>
                <div className="flex gap-2">
                  <Button onClick={() => handleLevelUp(hero.id)} disabled={gold - spentGold < cost}>
                    Level Up <span className="ml-2 font-mono text-xs">({cost}g)</span>
                  </Button>
                </div>
              </Card>
            )
          })}
        </CardContent>
        <CardFooter className="flex justify-center mt-4">
          <Button onClick={handleFinishUpgrading} size="lg" className="text-lg">
            Start Next Day
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
