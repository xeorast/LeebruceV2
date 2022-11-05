export const customMatchers: jasmine.CustomMatcherFactories = {
    noCallsHere: function ( _matchersUtil: jasmine.MatchersUtil ) {
        return {
            compare: function ( _actual: any, _expected: any ) {
                const result: jasmine.CustomMatcherResult = {
                    pass: false,
                    message: 'Function expected not to be called was called'
                };
                return result;
            }
        };
    }
};