module.exports = ({env}) => ({
    host: env("HOST"),
    port: env.int("PORT"),
    cron: {
        enabled: false,
    },
    admin: {
        autoOpen: false,
        url: env("ADMIN_URL", "/admin"),
        auth: {
            secret: env("JWT_SECRET"),
        },
    },
});
