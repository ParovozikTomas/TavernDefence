'use client';
import type { Enemy as EnemyType } from '@/lib/game-types';
import { Progress } from '@/components/ui/progress';

interface EnemyProps {
  enemy: EnemyType;
}

export default function Enemy({ enemy }: EnemyProps) {
  const healthPercentage = (enemy.health / enemy.maxHealth) * 100;

  return (
    <div
      className="absolute flex flex-col items-center transition-all duration-500 ease-linear"
      style={{
        left: `${enemy.position.x}%`,
        top: `${enemy.position.y}%`,
        transform: 'translate(-50%, -50%)',
      }}
    >
      <div className="text-3xl">👹</div>
      <Progress value={healthPercentage} className="w-10 h-1 mt-1 bg-red-800/20" />
    </div>
  );
}
