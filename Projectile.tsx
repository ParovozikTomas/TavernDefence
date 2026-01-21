'use client';
import type { Projectile as ProjectileType } from '@/lib/game-types';

interface ProjectileProps {
  projectile: ProjectileType;
}

export default function Projectile({ projectile }: ProjectileProps) {
  return (
    <div
      className="absolute text-lg"
      style={{
        left: `${projectile.position.x}%`,
        top: `${projectile.position.y}%`,
        transform: 'translate(-50%, -50%)',
      }}
    >
      {projectile.visual}
    </div>
  );
}
