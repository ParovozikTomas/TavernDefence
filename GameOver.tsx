'use client';
import type { Hero } from '@/lib/game-types';
import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

interface GameOverProps {
  score: number;
  onRestart: () => void;
  totalKills: number;
  heroes: Hero[];
}

export default function GameOver({ score, onRestart, totalKills, heroes }: GameOverProps) {
  const sortedHeroes = [...heroes].sort((a, b) => (b.damageDealt || 0) - (a.damageDealt || 0));
  
  return (
    <div className="absolute inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50">
      <Card className="w-full max-w-lg text-center font-headline bg-card/90">
        <CardHeader>
          <CardTitle className="text-4xl text-primary">Game Over</CardTitle>
          <CardDescription>The tavern has fallen...</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <p className="text-lg">You survived for</p>
            <p className="text-6xl font-bold my-2">{score}</p>
            <p className="text-lg">{score === 1 ? 'day' : 'days'}.</p>
          </div>

          <div className="text-2xl font-bold flex items-center justify-center gap-2">
            <span role="img" aria-label="Kills">💀</span> {totalKills} Enemies Defeated
          </div>

          <div>
            <h3 className="text-xl font-bold text-primary flex items-center justify-center gap-2"><span role="img" aria-label="Stats">📊</span> Guardian Stats</h3>
            <div className="space-y-2 mt-2 text-left">
              {sortedHeroes.map((hero, index) => (
                <Card key={hero.id} className="p-2 bg-background/50">
                  <div className="flex justify-between items-center">
                    <span className="font-bold">{index + 1}. {hero.heroClass} (Lvl {hero.level}) {hero.isDead ? '💀' : ''}</span>
                    <div className="flex gap-4 text-sm">
                      <span className="flex items-center gap-1"><span role="img" aria-label="Damage">⚔️</span> {Math.round(hero.damageDealt || 0)}</span>
                      <span className="flex items-center gap-1"><span role="img" aria-label="Kills">💀</span> {hero.kills || 0}</span>
                    </div>
                  </div>
                </Card>
              ))}
            </div>
          </div>

        </CardContent>
        <CardFooter className="flex justify-center">
          <Button onClick={onRestart} size="lg" className="font-headline text-lg">Try Again</Button>
        </CardFooter>
      </Card>
    </div>
  );
}
