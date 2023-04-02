import { TestBed } from '@angular/core/testing';

import { AuthenticationInterceptor } from './authentication.interceptor';
import { AuthenticationService } from './authentication.service';

describe( 'AuthenticationInterceptor', () => {
  beforeEach( () => TestBed.configureTestingModule( {
    providers: [
      AuthenticationInterceptor,
      {
        provide: AuthenticationService,
        useValue: jasmine.createSpyObj( typeof AuthenticationService, [], ['token'] )
      }
    ]
  } ) );

  it( 'should be created', () => {
    const interceptor: AuthenticationInterceptor = TestBed.inject( AuthenticationInterceptor );
    expect( interceptor ).toBeTruthy();
  } );
} );
