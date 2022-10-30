import { TestBed } from '@angular/core/testing';

import { ErrorInterceptor } from './http-error-mapper.interceptor';

describe( 'ErrorInterceptor', () => {
  beforeEach( () => TestBed.configureTestingModule( {
    providers: [
      ErrorInterceptor
    ]
  } ) );

  it( 'should be created', () => {
    const interceptor: ErrorInterceptor = TestBed.inject( ErrorInterceptor );
    expect( interceptor ).toBeTruthy();
  } );
} );
