#import "WebViewController.h"

@implementation WebViewController

// MARK: - Life Cycle

- (void)viewDidLoad {
    [super viewDidLoad];
    
    [self setupWebView];
}


// MARK: - Setup

- (void)setupWebView {
    [self setWebView:[WKWebView new]];
    [[self webView] setAllowsBackForwardNavigationGestures:YES];
    [[self webView] loadRequest:[[NSURLRequest alloc] initWithURL:[self url]]];
    
    [[self view] addSubview:[self webView]];
    [[self webView] setTranslatesAutoresizingMaskIntoConstraints:NO];
    [[[[self webView] topAnchor] constraintEqualToAnchor:[[self view] topAnchor]] setActive:YES];
    [[[[self webView] leadingAnchor] constraintEqualToAnchor:[[self view] leadingAnchor]] setActive:YES];
    [[[[self webView] trailingAnchor] constraintEqualToAnchor:[[self view] trailingAnchor]] setActive:YES];
    [[[[self webView] bottomAnchor] constraintEqualToAnchor:[[self view] bottomAnchor]] setActive:YES];
}


// MARK: - Configure

- (void)configureWithURL:(NSURL *)url {
    [self setUrl:url];
}

@end
