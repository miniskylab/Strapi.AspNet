module.exports = ({ env }) => ({
  defaultConnection: "default",
  connections: {
    default: {
      connector: "bookshelf",
      settings: {
        client: env("DATABASE_CLIENT"),
        filename: env("DATABASE_FILENAME"),
        host: env("DATABASE_HOST",),
        port: env.int("DATABASE_PORT"),
        database: env("DATABASE_NAME"),
        username: env("DATABASE_USERNAME"),
        password: env("DATABASE_PASSWORD")
      },
      options: {
        useNullAsDefault: env.bool("DATABASE_USE_NULL_AS_DEFAULT")
      },
    },
  },
});
