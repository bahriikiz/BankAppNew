import { TestBed } from '@angular/core/testing';

import { Vakifbank } from './vakifbank';

describe('Vakifbank', () => {
  let service: Vakifbank;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Vakifbank);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
