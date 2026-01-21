'use client';
import { useState } from 'react';
import type { Hero, HeroClass, Skill } from '@/lib/game-types';
import { HERO_CLASSES, SKILLS } from '@/lib/game-data';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';

interface HeroSetupProps {
  onSetupComplete: (heroes: Hero[]) => void;
}

export default function HeroSetup({ onSetupComplete }: HeroSetupProps) {
  const [step, setStep] = useState<'hero' | 'skill'>('hero');
  const [team, setTeam] = useState<HeroClass[]>([]);
  const [finalHeroes, setFinalHeroes] = useState<Hero[]>([]);
  const [currentHeroIndex, setCurrentHeroIndex] = useState(0);

  const handleAddHero = (heroClass: HeroClass) => {
    if (team.length < 5) {
      console.log(`Player action: Add hero '${heroClass}' to team.`);
      setTeam([...team, heroClass]);
    }
  };

  const handleRemoveHero = (index: number) => {
    console.log(`Player action: Remove hero '${team[index]}' from team.`);
    setTeam(team.filter((_, i) => i !== index));
  };
  
  const confirmHeroSelection = () => {
    if (team.length === 5) {
      console.log(`Player action: Confirm hero selection. Team: ${team.join(', ')}`);
      setStep('skill');
    }
  }

  const handleSelectSkill = (skill: Skill) => {
    const heroClass = team[currentHeroIndex];
    console.log(`Player action: Select skill '${skill.name}' for ${heroClass}.`);

    const getRange = (hc: HeroClass) => {
      switch(hc) {
        case 'Warrior': return 150; // Sight range
        case 'Mage': return 300;
        case 'Archer': return 400;
        default: return 100;
      }
    };

    const getAttackSpeed = (hc: HeroClass) => {
      switch(hc) {
        case 'Mage': return 0.5;
        case 'Archer': return 1;
        case 'Warrior': return 0.8;
        default: return 1;
      }
    };
    
    const position = { x: 20 + currentHeroIndex * 15, y: 75 };

    const newHero: Hero = {
      id: `hero-${currentHeroIndex}-${Math.random()}`,
      heroClass,
      skill,
      level: 1,
      position: position,
      basePosition: position,
      health: 100,
      maxHealth: 100,
      attackDamage: 10,
      attackSpeed: getAttackSpeed(heroClass),
      range: getRange(heroClass),
      damageDealt: 0,
      kills: 0,
      upgrades: { damage: 0, health: 0, speed: 0 },
      targetEnemyId: null,
      attackCooldown: 0,
    };
    
    const updatedFinalHeroes = [...finalHeroes, newHero];
    setFinalHeroes(updatedFinalHeroes);

    if (currentHeroIndex < 4) {
      setCurrentHeroIndex(currentHeroIndex + 1);
    } else {
      onSetupComplete(updatedFinalHeroes);
    }
  };
  
  const renderHeroSelection = () => (
    <Card className="w-full max-w-4xl bg-card/80 backdrop-blur-sm">
      <CardHeader>
        <CardTitle className="text-4xl font-headline text-center">Assemble Your Guardians</CardTitle>
        <CardDescription className="text-center font-headline">Choose 5 heroes to defend the tavern.</CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-8">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {HERO_CLASSES.map(hc => {
            return (
              <Card key={hc.name} className="flex flex-col">
                <CardHeader className="items-center">
                   <div className="p-2 bg-muted rounded-full w-16 h-16 flex items-center justify-center text-4xl">
                      {hc.icon}
                   </div>
                  <CardTitle className="font-headline">{hc.name}</CardTitle>
                </CardHeader>
                <CardContent className="text-center flex-grow">
                  <p>{hc.description}</p>
                </CardContent>
                <CardFooter className="justify-center">
                  <Button onClick={() => handleAddHero(hc.name as HeroClass)} disabled={team.length >= 5}>
                    Add to Team
                  </Button>
                </CardFooter>
              </Card>
            )
          })}
        </div>
        <Card className="p-4">
            <h3 className="text-center font-headline text-xl mb-2">Your Team ({team.length}/5)</h3>
            <div className="flex justify-center items-center gap-4 min-h-[60px] flex-wrap">
                {team.map((heroClass, index) => {
                    const heroClassData = HERO_CLASSES.find(hc => hc.name === heroClass);
                    return (
                        <Badge key={index} variant="secondary" className="p-2 text-md font-headline cursor-pointer" onClick={() => handleRemoveHero(index)}>
                            <span className="mr-2 text-lg">{heroClassData?.icon}</span>
                            {heroClass}
                        </Badge>
                    );
                })}
                {team.length === 0 && <p className="text-muted-foreground">Select heroes from above</p>}
            </div>
        </Card>
      </CardContent>
      <CardFooter className="justify-center">
        <Button size="lg" disabled={team.length !== 5} onClick={confirmHeroSelection}>
          Confirm Team <span className="ml-2">→</span>
        </Button>
      </CardFooter>
    </Card>
  );

  const renderSkillSelection = () => {
    const currentHeroClass = team[currentHeroIndex];
    const availableSkills = SKILLS[currentHeroClass];
    return (
      <Card className="w-full max-w-4xl bg-card/80 backdrop-blur-sm">
        <CardHeader>
          <CardTitle className="text-3xl font-headline text-center">
            Choose a Skill for Your {currentHeroClass}
          </CardTitle>
          <CardDescription className="text-center font-headline">
            Hero {currentHeroIndex + 1} of 5
          </CardDescription>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-4">
            {availableSkills.map(skill => (
                <Card key={skill.id} className="cursor-pointer hover:border-primary transition-colors" onClick={() => handleSelectSkill(skill)}>
                    <CardHeader className="items-center p-4">
                        <div className="p-2 bg-muted rounded-full text-2xl">{skill.icon}</div>
                        <CardTitle className="font-headline text-lg text-center mt-2">{skill.name}</CardTitle>
                    </CardHeader>
                    <CardContent className="text-sm text-center p-4 pt-0">
                        {skill.description}
                    </CardContent>
                </Card>
            ))}
        </CardContent>
      </Card>
    );
  };


  return (
    <div className="absolute inset-0 flex items-center justify-center p-4">
        {step === 'hero' ? renderHeroSelection() : renderSkillSelection()}
    </div>
  );
}
