import { Component, Input } from '@angular/core';
import { DisplayParameter } from '../../model';
import { VuwInput } from '../vuw-input/vuw-input';

@Component({
  selector: 'app-parameter-field',
  imports: [VuwInput],
  templateUrl: './parameter-field.html',
  styleUrl: './parameter-field.css',
})
export class ParameterField {
  @Input({ required: true }) parameter!: DisplayParameter;
  @Input({ required: true }) title!: string;

  changeValue(e: Event) {
    const input = e.target as HTMLInputElement;
    let value = parseFloat(input.value);
    if (value > this.parameter.range.max) value = this.parameter.range.max;
    if (value < this.parameter.range.min) value = this.parameter.range.min;

    input.value = value.toString();

    this.parameter.value = value;

    fetch('/api/solver/Verlet/parameters', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        parameterName: this.title,
        value: this.parameter.value,
      }),
    });
  }
}
