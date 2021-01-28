module.exports = ({env}) => ({
    settings: {
        favicon: {
            path: "favicon.ico",
            maxAge: 86400000
        },
        public: {
            path: env("LOCAL_BLOB_STORAGE_DIRECTORY"),
            maxAge: 60000
        }
    }
});