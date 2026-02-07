import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SensitiveField } from '../../core/models/models';

@Component({
  selector: 'app-sensitive-field',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex items-center gap-2">
      <span class="font-mono text-sm" [class.text-[var(--text-secondary)]]="!field.isRevealed">
        @if (isRevealing()) {
          <span class="animate-pulse-soft">Loading...</span>
        } @else if (field.isRevealed && field.value) {
          {{ field.value }}
        } @else {
          {{ field.maskedValue }}
        }
      </span>
      
      @if (field.canReveal && !field.isRevealed) {
        <button 
          (click)="onReveal()"
          [disabled]="isRevealing()"
          class="p-1.5 rounded-lg hover:bg-[var(--background)] text-[var(--text-secondary)] hover:text-emerald-600 transition-colors disabled:opacity-50"
          title="Reveal value">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
          </svg>
        </button>
      }
      
      @if (field.isRevealed) {
        <button 
          (click)="onHide()"
          class="p-1.5 rounded-lg hover:bg-[var(--background)] text-emerald-600 hover:text-[var(--text-secondary)] transition-colors"
          title="Hide value">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
          </svg>
        </button>
      }
    </div>
  `
})
export class SensitiveFieldComponent {
  @Input() field: SensitiveField = { maskedValue: '********', isRevealed: false, canReveal: false };
  @Output() reveal = new EventEmitter<void>();
  @Output() hide = new EventEmitter<void>();

  isRevealing = signal(false);

  onReveal(): void {
    this.isRevealing.set(true);
    this.reveal.emit();
    // Parent component should update field.isRevealed and field.value
    setTimeout(() => this.isRevealing.set(false), 2000); // Timeout fallback
  }

  onHide(): void {
    this.hide.emit();
  }
}
