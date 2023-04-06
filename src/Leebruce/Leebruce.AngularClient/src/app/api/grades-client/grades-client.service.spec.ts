import { TestBed } from '@angular/core/testing';

import { GradesClientService } from './grades-client.service';

describe('GradesClientService', () => {
  let service: GradesClientService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GradesClientService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
