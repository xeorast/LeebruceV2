import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { AuthenticationService, loginDto } from './authentication.service';

describe( 'AuthenticationService', () => {
  let service: AuthenticationService;
  let httpMock: HttpTestingController;

  beforeEach( () => {
    TestBed.configureTestingModule( {
      imports: [
        HttpClientTestingModule,
      ],
      providers: [
        AuthenticationService
      ]

    } );
    service = TestBed.inject( AuthenticationService );
    httpMock = TestBed.inject( HttpTestingController )
  } );

  it( 'should be created', () => {
    expect( service ).toBeTruthy();
  } );

  it( 'should post credentials', () => {
    // arrange
    var credentials = <loginDto>{ username: "user1", password: "password2" };
    var token = 'token3'

    // act
    service.logIn( credentials )
      .subscribe( { next: token => expect( token ).toBe( token ) } );

    // assert
    var req = httpMock.expectOne( 'api/auth/login' )
    req.flush( token )

    expect( req.request.method ).toBe( 'POST' );
    expect( req.request.body ).toEqual( credentials );
  } );

  it( 'should save token', () => {
    // arrange
    var credentials = <loginDto>{ username: "user1", password: "password2" };
    var token = 'token3'

    // act
    service.logIn( credentials )
      .subscribe();

    // assert
    var req = httpMock.expectOne( 'api/auth/login' )
    req.flush( token )

    expect( service.token ).toBe( token );
  } );
} );
