import { AfterViewInit, Component, inject, Input, OnDestroy } from '@angular/core';
import { Solvers } from '../../services/solvers';

@Component({
  selector: 'app-solver-simulation',
  templateUrl: './solver-simulation.html',
  styleUrl: './solver-simulation.css',
})
export class SolverSimulation implements AfterViewInit, OnDestroy {
  @Input({ required: true }) solver!: string;
  state = '';

  private canvas!: HTMLCanvasElement;
  private ctx!: CanvasRenderingContext2D;
  private solversService = inject(Solvers);

  private readonly padding = 20;

  private updateTimer: any;
  ngAfterViewInit() {
    this.canvas = document.querySelector('canvas')!;
    this.ctx = this.canvas.getContext('2d')!;
    this.requestNewFrame();

    this.updateTimer = setInterval(() => {
      this.requestNewFrame();
    }, 1000 / 30); // 30 FPS
  }

  requestNewFrame() {
    this.solversService.requestUpdate(this.solver, this.state).then((data) => {
      this.state = data;
      // ---- CLEAR & DRAW ----
      this.ctx.fillStyle = '#fafafa';
      this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

      // Draw boundary
      this.ctx.strokeStyle = '#bbb';
      this.ctx.strokeRect(
        this.padding,
        this.padding,
        this.canvas.width - 2 * this.padding,
        this.canvas.height - 2 * this.padding
      );

      const bytes = this.base64ToBytes(data);
      const view = new DataView(bytes.buffer);
      let offset = 0;

      // container dimensions (AFTER particles)
      const width = view.getFloat32(offset, true);
      offset += 4;
      const height = view.getFloat32(offset, true);
      offset += 4;
      const depth = view.getFloat32(offset, true);
      offset += 4;

      // int32 particle count
      const numParticles = view.getInt32(offset, true);
      offset += 4;

      // particles
      this.ctx.strokeStyle = 'blue';
      for (let i = 0; i < numParticles; i++) {
        const mass = view.getFloat32(offset, true);
        offset += 4;

        const px = view.getFloat32(offset, true);
        offset += 4;
        const py = view.getFloat32(offset, true);
        offset += 4;
        const pz = view.getFloat32(offset, true);
        offset += 4;

        const vx = view.getFloat32(offset, true);
        offset += 4;
        const vy = view.getFloat32(offset, true);
        offset += 4;
        const vz = view.getFloat32(offset, true);
        offset += 4;

        // draw particle (example)
        const x = this.padding + (px / width) * (this.canvas.width - 2 * this.padding);
        const y = this.padding + (1 - py / height) * (this.canvas.height - 2 * this.padding);

        const radius = 6;

        // draw circle
        this.ctx.beginPath();
        this.ctx.arc(x, y, radius, 0, Math.PI * 2);
        this.ctx.stroke();
      }
    });
  }

  ngOnDestroy() {
    clearInterval(this.updateTimer);
  }

  private base64ToBytes(b64: string): Uint8Array {
    const binary = atob(b64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
      bytes[i] = binary.charCodeAt(i);
    }
    return bytes;
  }
}
