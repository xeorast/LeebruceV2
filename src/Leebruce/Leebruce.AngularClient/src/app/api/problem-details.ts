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
