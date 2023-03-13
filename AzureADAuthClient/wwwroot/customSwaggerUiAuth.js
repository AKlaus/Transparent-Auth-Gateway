/*
 * Replaces the `response_type` parameter from the default `token` to `id_token`.
 * Based on this answer https://stackoverflow.com/a/68260111/968003
 */

if (!window.isOpenReplaced) {
    window.open = function (open) {
        return function (url) {
            url = url.replace('response_type=token', 'response_type=id_token');
            return open.call(window, url);
        };
    }(window.open);
    window.isOpenReplaced = true;
}