'use client';
import type { Hero as HeroType } from '@/lib/game-types';
import { Progress } from '@/components/ui/progress';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { HERO_CLASSES } from '@/lib/game-data';
import { cn } from '@/lib/utils';

interface HeroProps {
  hero: HeroType;
  onMouseDown: (id: string, e: React.MouseEvent) => void;
  isBeingDragged: boolean;
}

export default function Hero({ hero, onMouseDown, isBeingDragged }: HeroProps) {
  const healthPercentage = (hero.health / hero.maxHealth) * 100;
  const heroClassData = HERO_CLASSES.find(hc => hc.name === hero.heroClass);

  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger asChild>
          <div
            className={cn(
              "absolute flex flex-col items-center transition-opacity duration-300 cursor-grab active:cursor-grabbing"
            )}
            style={{
              left: `${hero.position.x}%`,
              top: `${hero.position.y}%`,
              transform: 'translate(-50%, -50%)',
              zIndex: isBeingDragged ? 10 : 5,
            }}
            onMouseDown={(e) => onMouseDown(hero.id, e)}
          >
            <div className={cn(
                'w-10 h-10 flex items-center justify-center bg-background/50 rounded-full border border-foreground/20 transition-all text-2xl',
                isBeingDragged && 'border-primary border-2 shadow-lg ring-2 ring-primary/50'
             )}>
              {heroClassData?.icon}
            </div>
            <Progress value={healthPercentage} className="w-12 h-1 mt-1" />
          </div>
        </TooltipTrigger>
        <TooltipContent className="font-headline">
          <p>{hero.heroClass} - Lvl {hero.level}</p>
          <p>HP: {hero.health.toFixed(0)}/{hero.maxHealth}</p>
          <p>Skill: {hero.skill.name}</p>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
}
