'use client';
import { Card, CardContent } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Button } from '@/components/ui/button';

interface GameUIProps {
  day: number;
  gold: number;
  tavernHealth: number;
  maxTavernHealth: number;
  gameSpeed: number;
  setGameSpeed: (speed: number) => void;
  totalKills: number;
}

export default function GameUI({ day, gold, tavernHealth, maxTavernHealth, gameSpeed, setGameSpeed, totalKills }: GameUIProps) {
  const healthPercentage = (tavernHealth / maxTavernHealth) * 100;

  return (
    <div className="absolute top-2 left-2 right-2 flex justify-between items-start z-10 gap-4">
      <Card className="bg-card/80 backdrop-blur-sm">
        <CardContent className="p-2 flex items-center gap-4">
          <div className="flex items-center gap-2">
            <span role="img" aria-label="Day">☀️</span>
            <span className="font-bold text-lg">{day}</span>
          </div>
          <div className="flex items-center gap-2">
            <span role="img" aria-label="Gold">💰</span>
            <span className="font-bold text-lg">{gold}</span>
          </div>
           <div className="flex items-center gap-2">
            <span role="img" aria-label="Total Kills">💀</span>
            <span className="font-bold text-lg">{totalKills}</span>
          </div>
          <div className="flex items-center gap-1 border-l border-border pl-2 ml-2">
            <Button variant={gameSpeed === 1 ? 'secondary' : 'ghost'} size="sm" onClick={() => setGameSpeed(1)} className="h-8 w-8 p-0">1x</Button>
            <Button variant={gameSpeed === 2 ? 'secondary' : 'ghost'} size="sm" onClick={() => setGameSpeed(2)} className="h-8 w-8 p-0">2x</Button>
            <Button variant={gameSpeed === 3 ? 'secondary' : 'ghost'} size="sm" onClick={() => setGameSpeed(3)} className="h-8 w-8 p-0">3x</Button>
          </div>
        </CardContent>
      </Card>
      
      <Card className="bg-card/80 backdrop-blur-sm w-1/3 max-w-xs">
        <CardContent className="p-2">
          <div className="flex items-center gap-2 text-primary font-bold">
            <span role="img" aria-label="Tavern Health">❤️</span>
            Tavern Health
          </div>
          <Progress value={healthPercentage} className="h-3 mt-1" />
        </CardContent>
      </Card>
    </div>
  );
}
