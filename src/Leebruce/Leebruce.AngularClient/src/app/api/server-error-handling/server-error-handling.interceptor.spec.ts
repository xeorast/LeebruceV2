import { TestBed } from '@angular/core/testing';

import { ServerErrorHandlingInterceptor } from './server-error-handling.interceptor';

describe('ServerErrorHandlingInterceptor', () => {
  beforeEach(() => TestBed.configureTestingModule({
    providers: [
      ServerErrorHandlingInterceptor
      ]
  }));

  it('should be created', () => {
    const interceptor: ServerErrorHandlingInterceptor = TestBed.inject(ServerErrorHandlingInterceptor);
    expect(interceptor).toBeTruthy();
  });
});
