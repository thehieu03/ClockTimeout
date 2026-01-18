-- Check migration history
SELECT "MigrationId", "ProductVersion" FROM "__EFMigrationsHistory";

-- Check table counts
SELECT 'Products' as table_name, COUNT(*) as row_count FROM "Products"
UNION ALL
SELECT 'ProductImages' as table_name, COUNT(*) as row_count FROM "ProductImages";

-- Check Products table structure
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name = 'Products'
ORDER BY ordinal_position;

-- Check ProductImages table structure
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name = 'ProductImages'
ORDER BY ordinal_position;

-- Check indexes on Products
SELECT 
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename = 'Products';

-- Check indexes on ProductImages
SELECT 
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename = 'ProductImages';
