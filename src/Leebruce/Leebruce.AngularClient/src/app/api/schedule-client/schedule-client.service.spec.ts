import { TestBed } from '@angular/core/testing';

import { ScheduleClientService } from './schedule-client.service';

describe('ScheduleClientService', () => {
  let service: ScheduleClientService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ScheduleClientService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
