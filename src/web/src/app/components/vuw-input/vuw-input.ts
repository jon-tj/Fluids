import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DisplayParameter } from '../../model';

@Component({
  selector: 'app-vuw-input',
  imports: [],
  templateUrl: './vuw-input.html',
  styleUrl: './vuw-input.css',
})
export class VuwInput {
  @Input({ required: true }) parameter!: DisplayParameter;
  @Output() change = new EventEmitter<Event>();

  changeValue(e: Event) {
    this.change.emit(e);
  }
}
