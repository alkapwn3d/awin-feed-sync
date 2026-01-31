-- Sample data for testing Awin Feed Sync

-- Insert sample advertisers
INSERT INTO advertisers (advertiser_id, name, status, default_commission_text, updated_at)
VALUES 
    (12345, 'Sample Tech Store', 'active', '5% commission on all sales', NOW()),
    (67890, 'Fashion Boutique', 'active', '10% commission + $5 bonus', NOW()),
    (11111, 'Home & Garden Co', 'active', '7.5% commission', NOW())
ON CONFLICT (advertiser_id) DO NOTHING;

-- Insert sample products
INSERT INTO products (
    advertiser_id, product_key, feed_product_id, sku, product_name, product_url, 
    image_url, price, currency, category, subcategory, commission_text, commission_rate,
    tracking_url, tracking_url_source, content_hash, last_seen_at, last_changed_at, last_updated_at
)
VALUES 
    (
        12345, 'TECH-LAPTOP-001', 'LP001', 'SKU-LP-001',
        'Premium Laptop 15" - Intel i7, 16GB RAM, 512GB SSD',
        'https://example.com/products/laptop-001',
        'https://example.com/images/laptop-001.jpg',
        1299.99, 'USD', 'Electronics', 'Laptops',
        '5% commission', 5.0,
        'https://www.awin1.com/cread.php?awinmid=12345&awinaffid=YOUR_ID&clickref=&p=https://example.com/products/laptop-001',
        'computed', 
        encode(sha256('TECH-LAPTOP-001-v1'::bytea), 'hex'),
        NOW(), NOW(), NOW()
    ),
    (
        12345, 'TECH-MOUSE-002', 'MS002', 'SKU-MS-002',
        'Wireless Gaming Mouse - RGB, 16000 DPI',
        'https://example.com/products/mouse-002',
        'https://example.com/images/mouse-002.jpg',
        79.99, 'USD', 'Electronics', 'Computer Accessories',
        '5% commission', 5.0,
        'https://www.awin1.com/cread.php?awinmid=12345&awinaffid=YOUR_ID&clickref=&p=https://example.com/products/mouse-002',
        'computed',
        encode(sha256('TECH-MOUSE-002-v1'::bytea), 'hex'),
        NOW(), NOW(), NOW()
    ),
    (
        67890, 'FASHION-DRESS-001', 'DR001', 'SKU-DR-001',
        'Summer Floral Dress - Size M',
        'https://example.com/products/dress-001',
        'https://example.com/images/dress-001.jpg',
        89.99, 'USD', 'Fashion', 'Dresses',
        '10% commission + $5 bonus', 10.0,
        'https://www.awin1.com/cread.php?awinmid=67890&awinaffid=YOUR_ID&clickref=&p=https://example.com/products/dress-001',
        'computed',
        encode(sha256('FASHION-DRESS-001-v1'::bytea), 'hex'),
        NOW(), NOW(), NOW()
    ),
    (
        67890, 'FASHION-SHOES-002', 'SH002', 'SKU-SH-002',
        'Leather Ankle Boots - Black, Size 8',
        'https://example.com/products/shoes-002',
        'https://example.com/images/shoes-002.jpg',
        129.99, 'USD', 'Fashion', 'Shoes',
        '10% commission + $5 bonus', 10.0,
        'https://www.awin1.com/cread.php?awinmid=67890&awinaffid=YOUR_ID&clickref=&p=https://example.com/products/shoes-002',
        'computed',
        encode(sha256('FASHION-SHOES-002-v1'::bytea), 'hex'),
        NOW(), NOW(), NOW()
    ),
    (
        11111, 'HOME-CHAIR-001', 'CH001', 'SKU-CH-001',
        'Ergonomic Office Chair - Mesh Back, Adjustable',
        'https://example.com/products/chair-001',
        'https://example.com/images/chair-001.jpg',
        249.99, 'USD', 'Home & Garden', 'Furniture',
        '7.5% commission', 7.5,
        'https://www.awin1.com/cread.php?awinmid=11111&awinaffid=YOUR_ID&clickref=&p=https://example.com/products/chair-001',
        'computed',
        encode(sha256('HOME-CHAIR-001-v1'::bytea), 'hex'),
        NOW(), NOW(), NOW()
    )
ON CONFLICT (advertiser_id, product_key) DO NOTHING;

-- Insert a sample sync run
INSERT INTO sync_runs (started_at, finished_at, status, advertisers_processed, products_seen, products_changed)
VALUES 
    (NOW() - INTERVAL '1 hour', NOW() - INTERVAL '55 minutes', 'completed', 3, 5, 5)
ON CONFLICT DO NOTHING;

-- Display summary
SELECT 'Advertisers' as table_name, COUNT(*) as count FROM advertisers
UNION ALL
SELECT 'Products', COUNT(*) FROM products
UNION ALL
SELECT 'Sync Runs', COUNT(*) FROM sync_runs;
