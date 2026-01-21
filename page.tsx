'use client';

import { useState, useMemo, useEffect } from 'react';
import type { Hero } from '@/lib/game-types';
import HeroSetup from '@/components/game/HeroSetup';
import GameScreen from '@/components/game/GameScreen';
import UpgradeShop from '@/components/game/UpgradeShop';
import GameOver from '@/components/game/GameOver';

export type GamePhase = 'setup' | 'wave' | 'upgrade' | 'game_over';

export default function TavernGuardiansGame() {
  const [gamePhase, setGamePhase] = useState<GamePhase>('setup');
  const [heroes, setHeroes] = useState<Hero[]>([]);
  const [day, setDay] = useState(1);
  const [gold, setGold] = useState(100);
  const [tavernHealth, setTavernHealth] = useState(1000);
  const [score, setScore] = useState(0);
  const [gameSpeed, setGameSpeed] = useState(1);
  const [totalKills, setTotalKills] = useState(0);

  useEffect(() => {
    console.log(`Game phase changed to: ${gamePhase}`);
  }, [gamePhase]);

  const handleSetupComplete = (selectedHeroes: Hero[]) => {
    console.log('Player action: Setup complete. Heroes:', selectedHeroes.map(h => h.heroClass));
    setHeroes(selectedHeroes);
    setGamePhase('wave');
  };

  const handleWaveComplete = (waveGold: number, finalHeroes: Hero[], killsInWave: number) => {
    console.log(`Wave ${day} complete. Earned ${waveGold} gold. Killed ${killsInWave} enemies.`);
    setGold(currentGold => currentGold + waveGold);
    setTotalKills(k => k + killsInWave);
    setScore(day);
    setHeroes(finalHeroes);
    setGamePhase('upgrade');
  };

  const handleUpgradesComplete = (updatedHeroes: Hero[], spentGold: number) => {
    console.log(`Player action: Upgrades complete. Spent ${spentGold} gold.`, updatedHeroes);
    const heroesForNextWave = updatedHeroes.map(hero => ({
      ...hero,
      // For living heroes, reset health to max for the next round. Dead heroes stay dead.
      health: hero.isDead ? 0 : hero.maxHealth,
    }));
    setHeroes(heroesForNextWave);
    setGold(currentGold => currentGold - spentGold);
    setDay(currentDay => currentDay + 1);
    setGamePhase('wave');
  };

  const handleGameOver = (finalHeroes: Hero[]) => {
    console.log(`Game Over. Final score (days survived): ${day}`);
    setScore(day);
    setHeroes(finalHeroes);
    setGamePhase('game_over');
  };

  const handleRestart = () => {
    console.log('Player action: Restarting game.');
    setGamePhase('setup');
    setHeroes([]);
    setDay(1);
    setGold(100);
    setTavernHealth(1000);
    setScore(0);
    setTotalKills(0);
    setGameSpeed(1);
  };
  
  const gameContext = useMemo(() => ({
    heroes,
    day,
    gold,
    tavernHealth,
    setTavernHealth,
    onWaveComplete: handleWaveComplete,
    onGameOver: handleGameOver,
  }), [heroes, day, gold, tavernHealth]);


  const renderGamePhase = () => {
    switch (gamePhase) {
      case 'setup':
        return <HeroSetup onSetupComplete={handleSetupComplete} />;
      case 'wave':
        return <GameScreen {...gameContext} gameSpeed={gameSpeed} setGameSpeed={setGameSpeed} totalKills={totalKills} />;
      case 'upgrade':
        return <UpgradeShop heroes={heroes} gold={gold} onUpgradesComplete={handleUpgradesComplete} />;
      case 'game_over':
        return <GameOver score={score} onRestart={handleRestart} heroes={heroes} totalKills={totalKills} />;
      default:
        return <HeroSetup onSetupComplete={handleSetupComplete} />;
    }
  }

  return (
    <main className="font-body antialiased bg-background text-foreground min-h-screen flex items-center justify-center p-4 overflow-hidden">
      <div className="w-full h-full max-w-7xl max-h-[90vh] aspect-[16/9] relative">
        {renderGamePhase()}
      </div>
    </main>
  );
}
