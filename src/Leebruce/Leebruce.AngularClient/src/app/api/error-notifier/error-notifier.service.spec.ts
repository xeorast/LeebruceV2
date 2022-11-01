import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';

import { ErrorNotifierService, ServerError } from './error-notifier.service';

describe( 'ErrorNotifierService', () => {
  let service: ErrorNotifierService;

  beforeEach( () => {
    TestBed.configureTestingModule( {} );
    service = TestBed.inject( ErrorNotifierService );
  } );

  it( 'should be created', () => {
    expect( service ).toBeTruthy();
  } );

  it( 'should raise error', () => {
    // arrange
    let error = new ServerError( new HttpErrorResponse( {} ), 'error occured because yes' )
    let called = 0

    // act
    service.errors.subscribe( publishedError => {
      ++called
      expect( publishedError ).toBe( error )
    } )

    service.raiseError( error )

    // assert
    expect( called ).toBe( 1 )

  } );
} );
