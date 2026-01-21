'use client';
import { useState, useRef, useEffect, useCallback } from 'react';
import type { Hero, Skill, ActiveSkill } from '@/lib/game-types';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { Loader } from 'lucide-react';
import { adjustHeroSkills, AdjustHeroSkillsOutput } from '@/ai/flows/reactive-hero-skills';
import { useToast } from '@/hooks/use-toast';
import { HERO_CLASSES } from '@/lib/game-data';

interface SkillBarProps {
  heroes: Hero[];
  onUseSkill: (skill: ActiveSkill) => void;
  gameAreaRef: React.RefObject<HTMLDivElement>;
}

const HeroIcon = ({ heroClass }: { heroClass: Hero['heroClass'] }) => {
  const heroClassData = HERO_CLASSES.find(hc => hc.name === heroClass);
  if (!heroClassData) return null;
  return <span className="text-lg">{heroClassData.icon}</span>;
};


export default function SkillBar({ heroes, onUseSkill, gameAreaRef }: SkillBarProps) {
  const { toast } = useToast();
  const [cooldowns, setCooldowns] = useState<Record<string, number>>({});
  const [isCasting, setIsCasting] = useState<string | null>(null);
  const [isThinking, setIsThinking] = useState(false);
  const [aiSuggestion, setAiSuggestion] = useState<AdjustHeroSkillsOutput | null>(null);

  const handleSkillClick = (hero: Hero) => {
    if ((cooldowns[hero.id] || 0) > 0 || isCasting) return;
    console.log(`Player action: Initiating cast for skill '${hero.skill.name}' by ${hero.heroClass}.`);
    setIsCasting(hero.id);
    toast({
      title: 'Cast Skill',
      description: `Click on the battlefield to cast ${hero.skill.name}.`,
    });
  };

  const handleGameAreaClick = useCallback((e: MouseEvent) => {
    if (!isCasting || !gameAreaRef.current) return;

    const hero = heroes.find(h => h.id === isCasting);
     if (!hero) {
        console.error("Error: Tried to cast a skill for a hero that could not be found.", { isCasting });
        setIsCasting(null);
        return;
    }
    
    const skill = hero.skill;
    const rect = gameAreaRef.current.getBoundingClientRect();
    const x = ((e.clientX - rect.left) / rect.width) * 100;
    const y = ((e.clientY - rect.top) / rect.height) * 100;
    
    console.log(`Player action: Casting skill '${skill.name}' at position (${x.toFixed(2)}, ${y.toFixed(2)}).`);
    onUseSkill({ ...skill, casterId: hero.id, position: { x, y }, timestamp: Date.now() });

    setIsCasting(null);
    setCooldowns(prev => ({ ...prev, [hero.id]: skill.cooldown }));

    const interval = setInterval(() => {
      setCooldowns(prev => {
        const newTime = (prev[hero.id] || 0) - 1;
        if (newTime <= 0) {
          clearInterval(interval);
          // Create a new object without the finished cooldown
          const { [hero.id]: _, ...rest } = prev;
          return rest;
        }
        return { ...prev, [hero.id]: newTime };
      });
    }, 1000);
  }, [isCasting, gameAreaRef, onUseSkill, heroes]);
  
  const handleAiSuggest = async () => {
    console.log('Player action: Requesting AI skill suggestion.');
    setIsThinking(true);
    setAiSuggestion(null);
    try {
      // Simplified game state for the AI
      const gameState = `Heroes: ${heroes.filter(h => !h.isDead).length}, Tavern Health: low. Enemies: many, clustered near tavern.`;
      const suggestion = await adjustHeroSkills({ gameState });
      setAiSuggestion(suggestion);
      console.log('AI Suggestion received:', suggestion);
      if (!suggestion.skillActivations || suggestion.skillActivations.length === 0) {
        toast({
          title: "AI Strategist:",
          description: "No specific suggestions at this time. Keep fighting!",
        });
        return;
      }
      toast({
        title: "AI Strategist Suggests:",
        description: suggestion.skillActivations[0]?.reason || "Use a skill!",
      });
    } catch (error) {
      console.error('AI Suggestion Error:', error);
      toast({
        variant: 'destructive',
        title: 'AI Error',
        description: 'The AI strategist is currently unavailable.',
      });
    } finally {
      setIsThinking(false);
    }
  };

  useEffect(() => {
    const gameArea = gameAreaRef.current;
    if (gameArea) {
      const clickHandler = (e: MouseEvent) => {
        if(isCasting) {
          handleGameAreaClick(e)
        }
      };
      gameArea.addEventListener('click', clickHandler);
      return () => {
          gameArea.removeEventListener('click', clickHandler);
      };
    }
  }, [isCasting, handleGameAreaClick, gameAreaRef]);
  
  return (
    <Card className="absolute bottom-2 left-1/2 -translate-x-1/2 bg-card/80 backdrop-blur-sm z-20">
      <CardContent className="p-2 flex items-center gap-2">
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <Button variant="secondary" onClick={handleAiSuggest} disabled={isThinking} className="bg-accent text-accent-foreground">
                {isThinking ? <Loader className="animate-spin" /> : <span className="text-2xl">🪄</span>}
              </Button>
            </TooltipTrigger>
            <TooltipContent><p>Ask AI for a hint</p></TooltipContent>
          </Tooltip>
        </TooltipProvider>

        {heroes.filter(h => !h.isDead).map((hero) => {
          const skill = hero.skill;
          const onCooldown = (cooldowns[hero.id] || 0) > 0;
          const isSuggested = aiSuggestion?.skillActivations.some(s => s.heroId === hero.id);
          
          return (
          <TooltipProvider key={hero.id}>
            <Tooltip>
              <TooltipTrigger asChild>
                <div className="relative">
                  <Button
                    variant={isCasting === hero.id ? "outline" : "ghost"}
                    size="icon"
                    className={`w-16 h-16 border-2 ${isCasting === hero.id ? 'border-primary' : ''} ${isSuggested ? 'animate-pulse border-primary ring-2 ring-primary/50' : ''}`}
                    onClick={() => handleSkillClick(hero)}
                    disabled={onCooldown || (isCasting !== null && isCasting !== hero.id)}
                  >
                    <div className="flex flex-col items-center justify-center">
                        <div className="text-2xl">{skill.icon}</div>
                        <span className="text-xs font-mono">{skill.name.substring(0,8)}</span>
                    </div>
                  </Button>
                   <div className="absolute top-0 right-0 -mt-2 -mr-2 bg-background rounded-full p-0.5 border">
                      <HeroIcon heroClass={hero.heroClass} />
                    </div>
                  {onCooldown && (
                    <div className="absolute inset-0 bg-black/70 flex items-center justify-center rounded-md pointer-events-none">
                      <span className="text-white font-bold text-2xl">{cooldowns[hero.id]}</span>
                    </div>
                  )}
                </div>
              </TooltipTrigger>
              <TooltipContent>
                <p className="font-bold">{skill.name} ({skill.cooldown}s)</p>
                <p>{skill.description}</p>
                <p className="text-muted-foreground">{hero.heroClass}</p>
              </TooltipContent>
            </Tooltip>
          </TooltipProvider>
        )})}
      </CardContent>
    </Card>
  );
}
