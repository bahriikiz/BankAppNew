import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BankList } from './bank-list';

describe('BankList', () => {
  let component: BankList;
  let fixture: ComponentFixture<BankList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BankList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BankList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
