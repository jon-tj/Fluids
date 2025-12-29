import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VuwInput } from './vuw-input';

describe('VuwInput', () => {
  let component: VuwInput;
  let fixture: ComponentFixture<VuwInput>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VuwInput]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VuwInput);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
