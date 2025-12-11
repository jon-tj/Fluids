import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ParameterField } from './parameter-field';

describe('ParameterField', () => {
  let component: ParameterField;
  let fixture: ComponentFixture<ParameterField>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ParameterField]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ParameterField);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
