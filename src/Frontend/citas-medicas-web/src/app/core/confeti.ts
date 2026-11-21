// Lluvia de confeti para celebrar (cumpleaños, primer ingreso...). Sin librerías.
export function lanzarConfeti(duracionMs = 3500): void {
  if (typeof window === 'undefined' || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

  const canvas = document.createElement('canvas');
  canvas.style.cssText = 'position:fixed;inset:0;width:100%;height:100%;pointer-events:none;z-index:9999';
  document.body.appendChild(canvas);

  const ctx = canvas.getContext('2d')!;
  const escala = window.devicePixelRatio || 1;
  canvas.width = window.innerWidth * escala;
  canvas.height = window.innerHeight * escala;
  ctx.scale(escala, escala);

  const colores = ['#14b8a6', '#f59e0b', '#ec4899', '#6366f1', '#22c55e', '#f43f5e', '#0ea5e9'];
  const piezas = Array.from({ length: 160 }, () => ({
    x: Math.random() * window.innerWidth,
    y: -20 - Math.random() * window.innerHeight * 0.6,
    ancho: 6 + Math.random() * 6,
    alto: 8 + Math.random() * 10,
    color: colores[Math.floor(Math.random() * colores.length)],
    vx: -1.5 + Math.random() * 3,
    vy: 2 + Math.random() * 3.5,
    giro: Math.random() * Math.PI,
    vGiro: -0.2 + Math.random() * 0.4
  }));

  const inicio = performance.now();
  const dibujar = (ahora: number) => {
    const t = ahora - inicio;
    ctx.clearRect(0, 0, window.innerWidth, window.innerHeight);
    ctx.globalAlpha = t > duracionMs - 800 ? Math.max(0, (duracionMs - t) / 800) : 1;

    for (const p of piezas) {
      p.x += p.vx;
      p.y += p.vy;
      p.giro += p.vGiro;
      ctx.save();
      ctx.translate(p.x, p.y);
      ctx.rotate(p.giro);
      ctx.fillStyle = p.color;
      ctx.fillRect(-p.ancho / 2, -p.alto / 2, p.ancho, p.alto * Math.cos(p.giro * 2));
      ctx.restore();
    }

    if (t < duracionMs) requestAnimationFrame(dibujar);
    else canvas.remove();
  };
  requestAnimationFrame(dibujar);
}
