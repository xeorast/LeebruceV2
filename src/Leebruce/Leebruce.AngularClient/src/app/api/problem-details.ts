import { HttpErrorResponse } from "@angular/common/http"

export class HttpError extends Error {
    constructor( response: HttpErrorResponse )
    constructor( response: HttpErrorResponse, message: string )
    constructor( public response: HttpErrorResponse, message?: string ) {
        super( message )
    }
}

export class HttpProblem extends HttpError {
    constructor(
        response: HttpErrorResponse,
        public details: ProblemDetails ) {
        super( response, ( details.detail ?? undefined ) as string )
    }
}

export class HttpValidationProblem extends HttpError {
    constructor(
        response: HttpErrorResponse,
        public details: ValidationProblemDetails ) {
        super( response, ( details.detail ?? undefined ) as string )
    }
}

export class ProblemDetails {
    type?: string | null = null
    title?: string | null = null
    status: number | null = null
    detail?: string | null = null
    instance?: string | null = null

    constructor();
    constructor( details: ProblemDetails );
    constructor( details?: ProblemDetails ) {
        if ( details === undefined ) {
            return;
        }

        this.type = details.type
        this.title = details.title
        this.status = details.status
        this.detail = details.detail
        this.instance = details.instance
    }

}

export class ValidationProblemDetails extends ProblemDetails {
    errors: { [id: string]: string[]; } = {}

    constructor()
    constructor( details: ValidationProblemDetails )
    constructor( details?: ValidationProblemDetails ) {
        super( details as ProblemDetails );
        if ( details === undefined ) {
            return;
        }

        this.errors = details.errors
    }

}
