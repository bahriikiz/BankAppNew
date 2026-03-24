import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DepositCalculator } from './deposit-calculator';

describe('DepositCalculator', () => {
  let component: DepositCalculator;
  let fixture: ComponentFixture<DepositCalculator>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DepositCalculator]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DepositCalculator);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
