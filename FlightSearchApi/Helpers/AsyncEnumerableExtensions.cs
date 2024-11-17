using System.Runtime.CompilerServices;

namespace FlightSearchApi.Helpers;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T> MergeStreams<T>(
        this IAsyncEnumerable<T> first,
        IAsyncEnumerable<T> second,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task<(bool hasMore, T? item)>>();
        var enumerators = new List<IAsyncEnumerator<T>>();
        var results = new List<T>();

        try
        {
            var firstEnumerator = first.GetAsyncEnumerator(cancellationToken);
            var secondEnumerator = second.GetAsyncEnumerator(cancellationToken);
            enumerators.Add(firstEnumerator);
            enumerators.Add(secondEnumerator);

            tasks.Add(GetNextAsync(firstEnumerator, cancellationToken));
            tasks.Add(GetNextAsync(secondEnumerator, cancellationToken));

            while (tasks.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                var completedTask = await Task.WhenAny(tasks);
                var index = tasks.IndexOf(completedTask);
                tasks.RemoveAt(index);

                var (hasMore, item) = await completedTask;
                
                if (hasMore && item != null)
                {
                    results.Add(item);
                    tasks.Add(GetNextAsync(enumerators[index], cancellationToken));
                }
            }
        }
        finally
        {
            foreach (var enumerator in enumerators)
            {
                await enumerator.DisposeAsync();
            }
        }

        // Yield results outside of try-catch
        foreach (var result in results)
        {
            yield return result;
        }
    }

    private static async Task<(bool hasMore, T? item)> GetNextAsync<T>(
        IAsyncEnumerator<T> enumerator,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await enumerator.MoveNextAsync())
            {
                return (true, enumerator.Current);
            }
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation
        }
        catch (Exception ex)
        {
            // Log error if needed
        }
        return (false, default);
    }
} 