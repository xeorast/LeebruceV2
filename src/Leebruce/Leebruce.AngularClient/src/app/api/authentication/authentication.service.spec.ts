import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { HttpErrorMapperModule } from '../http-error-mapper/http-error-mapper.module';
import { ProblemDetails } from '../problem-details';
import { ServerErrorHandlingModule } from '../server-error-handling/server-error-handling.module';
import { AuthenticationService, InvalidCredentialsError, loginDto } from './authentication.service';

import { customMatchers } from 'src/testing/matchers/custom-matchers';
import 'src/testing/matchers/custom-matchers-types';

describe( 'AuthenticationService', () => {
  let service: AuthenticationService;
  let httpMock: HttpTestingController;

  beforeEach( () => {
    jasmine.addMatchers( customMatchers )
    TestBed.configureTestingModule( {
      imports: [
        HttpClientTestingModule,
        ServerErrorHandlingModule,
        HttpErrorMapperModule,
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

  it( 'should throw on 401', () => {
    // arrange
    var credentials = <loginDto>{ username: "user1", password: "password2" };
    let details = new ProblemDetails( { status: 401, detail: 'some detail' } )

    // act
    service.logIn( credentials )
      .subscribe( {
        next: _ => expect().noCallsHere(),
        error: error => {
          expect( error ).toBeInstanceOf( InvalidCredentialsError )
          expect( ( error as InvalidCredentialsError ).message ).toBe( details.detail as string )
        }
      } );

    // assert
    var req = httpMock.expectOne( 'api/auth/login' )
    req.flush( details, { status: 401, statusText: 'Unauthorized' } )
  } );
} );
