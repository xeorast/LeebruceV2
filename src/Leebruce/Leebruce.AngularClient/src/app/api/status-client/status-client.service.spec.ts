import { TestBed } from '@angular/core/testing';

import { StatusClientService } from './status-client.service';

describe('StatusClientService', () => {
  let service: StatusClientService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(StatusClientService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
