import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MoneyTransfer } from './money-transfer';

describe('MoneyTransfer', () => {
  let component: MoneyTransfer;
  let fixture: ComponentFixture<MoneyTransfer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MoneyTransfer]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MoneyTransfer);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
